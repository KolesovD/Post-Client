using System;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace JohnGietzen.MailUtilities.Pop.Server
{
    internal class PopServerClientConnection
    {
        private PopServer Parent;
        private TcpClient Client;
        private Thread ClientThread;
        private IMailboxProvider MailboxProvider;
        private MessageList MessageList;
        private Guid ConnectionId = Guid.NewGuid();

        public Boolean Active
        {
            get
            {
                return (Client != null);
            }
        }

        /// <summary>
        /// Creates a new PopClientConnection, using <paramref name="client"/> for
        /// communication with the client, and <paramref name="mailboxProvider"/> for
        /// retrieval and management of messages.
        /// </summary>
        /// <param name="client">The client connection with which to communicate.</param>
        /// <param name="mailboxProvider">The mailbox provider to use.</param>
        public PopServerClientConnection(PopServer parent, TcpClient client, IMailboxProvider mailboxProvider)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (client == null)
                throw new ArgumentNullException("client");

            Parent = parent;

            MailboxProvider = mailboxProvider;

            Client = client;
            ClientThread = new Thread(new ThreadStart(Process));
            ClientThread.IsBackground = true;
            ClientThread.Start();
        }

        private enum AuthorizationStateResult
        {
            Success,
            Quit,
            Disconnected,
        }

        private enum TransactionStateResult
        {
            Quit,
            Disconnected,
            Timeout,
        }

        private class Locker : IDisposable
        {
            private Mutex Lock;
            
            public String Name { get { return _Name; } }
            private String _Name;
        
            public Boolean Acquire(string lockName)
            {
                if (Lock != null) return false;
                Lock = new Mutex(true, lockName);
                
                bool Acquired = false;
                try
                {
                    Acquired = Lock.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    Acquired = true;
                }
                
                if (Acquired)
                    _Name = lockName;
                
                return Acquired;
            }

            public void Release()
            {
                if (Lock == null)
                    return;
                Lock.ReleaseMutex();
                Lock.Close();
                Lock = null;
                _Name = null;
            }

            #region IDisposable Members

            public void Dispose()
            {
                Release();
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        private void Process()
        {
            Locker Lock = new Locker();
            try
            {
                {
                    ClientConnectedEventArgs args = new ClientConnectedEventArgs();
                    args.ConnectionId = ConnectionId;
                    args.RemoteEndpoint = (IPEndPoint)Client.Client.RemoteEndPoint;
                    Parent.RaiseClientConnected(this, args);
                }

                NetworkStream ClientStream = Client.GetStream();
                AuthorizationStateResult AuthorizationResult = AuthorizationState(ClientStream, Lock);
                if (AuthorizationResult == AuthorizationStateResult.Disconnected ||
                    AuthorizationResult == AuthorizationStateResult.Quit)
                {
                    return;
                }

                {
                    ClientAuthenticatedEventArgs args = new ClientAuthenticatedEventArgs();
                    args.ConnectionId = ConnectionId;
                    args.Account = Lock.Name;
                    Parent.RaiseClientAuthenticated(this, args);
                }

                TransactionStateResult TransactionResult = TransactionState(ClientStream);
                if (TransactionResult == TransactionStateResult.Disconnected ||
                    TransactionResult == TransactionStateResult.Timeout)
                {
                    return;
                }

                // Process transaction
                // TODO: Thread.Abort Proof This
                if (MessageList != null)
                {
                    MessageList.Process();
                }
            }
            catch (ThreadAbortException)
            {
            }
            finally
            {
                Lock.Release();
                Client.Client.Close();
                Client.Close();
                Client = null;
                
                {
                    ClientDisconnectedEventArgs args = new ClientDisconnectedEventArgs();
                    args.ConnectionId = ConnectionId;
                    Parent.RaiseClientDisconnected(this, args);
                }
            }
        }

        private AuthorizationStateResult AuthorizationState(NetworkStream ClientStream, Locker mailLock)
        {
            string Challenge = "<" + System.Diagnostics.Process.GetCurrentProcess().Id + "." + Environment.TickCount + "@" + Environment.MachineName + ">";
            Utilities.SendResponse(ClientStream, "+OK CSPop POP3 server ready " + Challenge + "\r\n");

            string Username = "";

            while (true)
            {
                ClientRequest Request = Utilities.GetClientRequest(ClientStream);
                if (Request == null) break;

                switch (Request.Command)
                {
                    case "AUTH":
                        Utilities.SendResponse(ClientStream, "-ERR Authentication not yet supported\r\n");
                        break;

                    case "APOP":
                        {
                            if (!this.MailboxProvider.PasswordsKnown)
                            {
                                goto default;
                            }

                            string[] Params = Request.Parameters.Split(new char[] { ' ' });
                            if (Params.Length != 2)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Invalid command format.\r\n");
                                break;
                            }

                            Username = Params[0];
                            IMailboxAccount Account = MailboxProvider.GetAccount(Username);
                            if (Account == null)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Bad username or password\r\n");
                                break;
                            }
                            byte[] hash = (new MD5CryptoServiceProvider()).ComputeHash(
                                            Encoding.UTF8.GetBytes(
                                                Challenge + Account.Password));
                            string passhash = BitConverter.ToString(hash).Replace("-", "");

                            if (passhash.ToUpperInvariant() != Params[1].ToUpperInvariant())
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Bad username or password\r\n");
                                break;
                            }

                            if (!mailLock.Acquire(Username))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Mailbox lock failed, in use\r\n");
                                break;
                            }

                            MessageList = Account.MessageList;

                            Utilities.SendResponse(ClientStream, "+OK Logged in, mailbox locked\r\n");
                            return AuthorizationStateResult.Success;
                        }

                    case "CAPA":
                        {
                            SendCapabilities(ClientStream, this.MailboxProvider.PasswordsKnown);
                        }
                        break;

                    case "USER":
                        {
                            Username = Request.Parameters;
                            Utilities.SendResponse(ClientStream, "+OK\r\n");
                        }
                        break;

                    case "PASS":
                        {
                            if (String.IsNullOrEmpty(Username))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Bad username or password\r\n");
                                break;
                            }

                            IMailboxAccount Account = MailboxProvider.GetAccount(Username);
                            if (Account == null)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Bad username or password\r\n");
                                break;
                            }

                            string Password = Request.Parameters;
                            if (!Account.ConfirmPassword(Password))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Bad username or password\r\n");
                                break;
                            }

                            if (!mailLock.Acquire(Username))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Mailbox lock failed, in use\r\n");
                                break;
                            }

                            MessageList = Account.MessageList;

                            Utilities.SendResponse(ClientStream, "+OK Logged in, mailbox locked\r\n");
                            return AuthorizationStateResult.Success;
                        }

                    case "QUIT":
                        Utilities.SendResponse(ClientStream, "+OK POP3 server signing off\r\n");
                        return AuthorizationStateResult.Quit;

                    default:
                        Utilities.SendResponse(ClientStream, "-ERR Unrecognized command\r\n");
                        break;
                }
            }

            return AuthorizationStateResult.Disconnected;
        }

        private TransactionStateResult TransactionState(NetworkStream ClientStream)
        {
            while (true)
            {
                ClientRequest Request = Utilities.GetClientRequest(ClientStream);
                if (Request == null) break;

                switch (Request.Command)
                {
                    case "CAPA":
                        SendCapabilities(ClientStream, this.MailboxProvider.PasswordsKnown);
                        break;

                    case "STAT":
                        Utilities.SendResponse(ClientStream, "+OK " + MessageList.Count + " " + MessageList.Size + "\r\n");
                        break;

                    case "LIST":
                        {
                            string[] p = Request.Parameters.Split(' ');
                            int Message = 0;
                            if (p.Length > 0 && int.TryParse(p[0], out Message))
                            {
                                if (Message < 1 || Message > MessageList.Count)
                                {
                                    Utilities.SendResponse(ClientStream, "-ERR No such message\r\n");
                                    break;
                                }
                                if (MessageList[Message].Deleted)
                                {
                                    Utilities.SendResponse(ClientStream, "-ERR Message has been deleted\r\n");
                                    break;
                                }

                                Utilities.SendResponse(ClientStream, "+OK " + Message + " " + MessageList[Message].Size + "\r\n");
                            }
                            else
                            {
                                Utilities.SendResponse(ClientStream, "+OK " + MessageList.Count + " " + MessageList.Size + "\r\n");
                                for (int i = 1; i <= MessageList.Count; i++)
                                {
                                    Utilities.SendResponse(ClientStream, i.ToString(CultureInfo.InvariantCulture) + " " + MessageList[i].Size + "\r\n");
                                }
                                Utilities.SendResponse(ClientStream, ".\r\n");
                            }
                        }
                        break;
                    case "UIDL":
                        {
                            string[] p = Request.Parameters.Split(' ');
                            int Message = 0;
                            if (p.Length > 0 && int.TryParse(p[0], out Message))
                            {
                                if (Message < 1 || Message > MessageList.Count)
                                {
                                    Utilities.SendResponse(ClientStream, "-ERR No such message\r\n");
                                    break;
                                }

                                Utilities.SendResponse(ClientStream, "+OK " + MessageList[Message].UIDL + "\r\n");
                            }
                            else
                            {
                                Utilities.SendResponse(ClientStream, "+OK\r\n");
                                for (int i = 1; i <= MessageList.Count; i++)
                                {
                                    Utilities.SendResponse(ClientStream, i.ToString(CultureInfo.InvariantCulture) + " " + MessageList[i].UIDL + "\r\n");
                                }
                                Utilities.SendResponse(ClientStream, ".\r\n");
                            }
                        }
                        break;
                    case "TOP":
                        {
                            string[] p = Request.Parameters.Split(' ');
                            int Message = 0, Lines = 0;
                            if (p.Length != 2 || !int.TryParse(p[0], out Message) || !int.TryParse(p[1], out Lines))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Unrecognized command\r\n");
                                break;
                            }
                            if (Message < 1 || Message > MessageList.Count)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR No such message\r\n");
                                break;
                            }
                            if (MessageList[Message].Deleted)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Message has been deleted\r\n");
                                break;
                            }

                            byte[] Data = Encoding.ASCII.GetBytes(MessageList[Message].MimeMessage.ToString());


                            Boolean headerdone = false;
                            int linesfound = 0;
                            int length = 0;
                            for (int i = 5; i < Data.Length; i++)
                            {
                                if (!headerdone)
                                {
                                    if (Data[i - 3] == 13 &&
                                        Data[i - 2] == 10 &&
                                        Data[i - 1] == 13 &&
                                        Data[i - 0] == 10)
                                    {
                                        headerdone = true;
                                    }
                                }
                                else
                                {
                                    if (Data[i - 1] == 13 &&
                                        Data[i - 0] == 10)
                                    {
                                        length = i;
                                        linesfound++;
                                        if (linesfound >= Lines)
                                            break;
                                    }
                                }
                            }

                            Utilities.SendResponse(ClientStream, "+OK Sending " + length + " octets\r\n");
                            ClientStream.Write(Data, 0, length);
                            if (length < Data.Length)
                            {
                                Utilities.SendResponse(ClientStream, "\r\n.\r\n");
                            }
                        }
                        break;

                    case "RETR":
                        {
                            string[] p = Request.Parameters.Split(' ');
                            int Message = 0;
                            if (p.Length != 1 || !int.TryParse(p[0], out Message))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Unrecognized command\r\n");
                                break;
                            }
                            if (Message < 1 || Message > MessageList.Count)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR No such message\r\n");
                                break;
                            }
                            if (MessageList[Message].Deleted)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Message has been deleted\r\n");
                                break;
                            }

                            byte[] Data = Encoding.UTF8.GetBytes(MessageList[Message].MimeMessage.ToString());

                            Utilities.SendResponse(ClientStream, "+OK Sending " + Data.Length + " octets\r\n");
                            ClientStream.Write(Data, 0, Data.Length);
                        }
                        break;

                    case "DELE":
                        {
                            string[] p = Request.Parameters.Split(' ');
                            int Message = 0;
                            if (p.Length != 1 || !int.TryParse(p[0], out Message))
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Unrecognized command\r\n");
                                break;
                            }
                            if (Message < 1 || Message > MessageList.Count)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR No such message\r\n");
                                break;
                            }
                            if (MessageList[Message].Deleted)
                            {
                                Utilities.SendResponse(ClientStream, "-ERR Message has already been deleted\r\n");
                                break;
                            }

                            MessageList[Message].Delete();
                            Utilities.SendResponse(ClientStream, "+OK Message " + Message + " deleted\r\n");
                        }
                        break;

                    case "QUIT":
                        Utilities.SendResponse(ClientStream, "+OK POP3 server signing off\r\n");
                        return TransactionStateResult.Quit;

                    default:
                        Utilities.SendResponse(ClientStream, "-ERR Unrecognized command\r\n");
                        break;
                }
            }

            return TransactionStateResult.Disconnected;
        }

        private static void SendCapabilities(NetworkStream stream, bool passwordsKnow)
        {
            Utilities.SendResponse(stream, "+OK Capability list follows\r\n");
            Utilities.SendResponse(stream, "TOP\r\n");
            Utilities.SendResponse(stream, "USER\r\n");
            Utilities.SendResponse(stream, "UIDL\r\n");
            if (passwordsKnow)
            {
                Utilities.SendResponse(stream, "APOP\r\n");
            }
            Utilities.SendResponse(stream, ".\r\n");
        }

        internal void Kill()
        {
            if (Active)
            {
                ClientThread.Abort();
                ClientThread.Join();
            }
        }
    }
}