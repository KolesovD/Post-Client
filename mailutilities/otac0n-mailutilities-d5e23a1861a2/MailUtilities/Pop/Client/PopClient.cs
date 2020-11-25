using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace JohnGietzen.MailUtilities.Pop.Client
{
    public class PopClientMessage : MailboxMessage
    {
        private PopClient Parent;
        private int MessageNumber;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PopClientMessage(PopClient parent, int messageNumber)
        {
            MessageNumber = messageNumber;
            Parent = parent;
        }

        public override void Delete()
        {
            Parent.Delete(MessageNumber);
            _Deleted = true;
        }

        public override Boolean Deleted
        {
            get { return _Deleted; }
        }
        private Boolean _Deleted = false;

        public override MimeMessage MimeMessage
        {
            get
            {
                return Parent.GetMimeMessage(MessageNumber);
            }
        }

        public override String UIDL
        {
            get
            {
                if (_Uidl == null)
                {
                    _Uidl = Parent.GetUidl(MessageNumber);
                }
                return _Uidl;
            }
        }
        private String _Uidl = null;

        public override void Process()
        {
            throw new InvalidOperationException("Processing of messages will only happen when the server is disconnected.");
        }

        public override long Size
        {
            get {
                if (_Size == -1)
                    _Size = Parent.GetSize(MessageNumber);
                return _Size; }
        }
        private long _Size = -1;
    }

    public class PopClient
    {
        private TcpClient Server;
        private Stream ServerStream;

        public Boolean Authenticated
        {
            get { return _Authenticated; }
        }
        private Boolean _Authenticated;

        private int MessageCount
        {
            get
            {
                if (Authenticated)
                {
                    if (_MessageCount == -1)
                    {
                        _MessageCount = GetMessageCount();
                    }
                    return _MessageCount;
                }
                else
                {
                    throw new InvalidOperationException("The client is not authenticated and cannot retrieve the message list.");
                }
            }
        }
        private int _MessageCount = -1;

        public MessageList Messages
        {
            get
            {
                if(Authenticated)
                {
                    if(_Messages == null)
                    {
                        _Messages = BuildMessageList();
                    }
                    return _Messages;
                }
                else
                {
                    throw new InvalidOperationException("The client is not authenticated and cannot retrieve the message list.");
                }
            }
        }
        private MessageList _Messages;

        public PopClient(String host, Int32 port, Boolean useSsl) { this.Connect(host, port, useSsl); }

        public PopClient() { }

        /// <summary>
        ///  Connects to the host specified by <paramref name="host"/> on port <paramref name="port"/>.
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <param name="host">The host name or IP address to connect to.</param>
        /// <param name="port">The port over which to connect.</param>
        public void Connect(String host, Int32 port, Boolean useSsl)
        {
            if (String.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");
            if (port > IPEndPoint.MaxPort || port < IPEndPoint.MinPort)
                throw new ArgumentOutOfRangeException("port");
            if (Server != null)
                throw new InvalidOperationException("Cannot connect until any previous connection is closed.");

            IPHostEntry HostEntry = Dns.GetHostEntry(host);
            TcpClient connection = new TcpClient();
            connection.Connect(new IPEndPoint(HostEntry.AddressList[0], port));

            if (!connection.Connected)
                throw new InvalidOperationException();

            _Messages = null;
            _MessageCount = -1;
            _Authenticated = false;

            if(useSsl)
            {
                SslStream SecureStream = new SslStream(
                    connection.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null);
                try
                {
                    SecureStream.AuthenticateAsClient(host);
                    ServerStream = SecureStream;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                ServerStream = connection.GetStream();
            }

            ServerResponse resp = Utilities.GetServerResponse(ServerStream);

            if (resp == null)
            {
                throw new ConnectionFailedException("The server did not return a response.");
            }
            else if (resp.ResponseLevel == ResponseLevel.ERR)
            {
                throw new ConnectionFailedException("The server returned an error response.");
            }

            Server = connection;
        }

        public static Boolean ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            else
                return false;
        }

        public void Disconnect(Boolean commitChanges)
        {
            // This instructs the client to throw an excaption if we are comitting changes, and the disconnect fails.
            Disconnect(commitChanges, commitChanges);
        }

        public void Disconnect(Boolean commitChanges, Boolean throwExceptionOnDirtyExit)
        {
            if (Server == null)
                throw new InvalidOperationException("Cannot disconnect when no connection exists.");

            Boolean CleanDisconnect = true;
            if (Server.Client.Connected)
            {
                if (commitChanges)
                {
                    ServerResponse resp;
                    Utilities.SendResponse(ServerStream, "QUIT\r\n");
                    resp = Utilities.GetServerResponse(ServerStream);
                    if (resp == null || resp.ResponseLevel != ResponseLevel.OK)
                    {
                        CleanDisconnect = false;
                    }
                }
                else
                {
                    // Just disconnect from the server without issuing the QUIT command.
                    // The server will rollback the transaction.
                }

                if (Server.Client.Connected)
                    Server.Client.Disconnect(false);
            }
            Server.Close();
            Server = null;
            _Messages = null;
            _MessageCount = -1;
            _Authenticated = false;

            if (!CleanDisconnect && throwExceptionOnDirtyExit)
            {
                throw new UncleanDisconnectException();
            }
        }


        public void Authenticate(String user, String password)
        {
            // TODO: Sanitize the username to not contain \r or \n (or spaces/tabs?)

            ServerResponse resp;

            // TODO: Determine the best authentication type

            
            // USER/PASS Authentication...
            Utilities.SendResponse(ServerStream, "USER " + user + "\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if(resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                throw new UserNameRejectedException("The username was rejected by the server.");
            }

            Utilities.SendResponse(ServerStream, "PASS " + password + "\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if (resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                if (resp.Message.IndexOf("lock", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    throw new MailboxLockFailedException("The server reported that a mailbox lock could not be acquired.");
                }
                else
                {
                    throw new PasswordRejectedException("The password was rejected by the server.");
                }
            }

            _Authenticated = true;
        }

        public Boolean TryAuthenticate(String user, String password)
        {
            try
            {
                this.Authenticate(user, password);
                return true;
            }
            catch (ConnectionFailedException)
            {
                return false;
            }
            catch (AuthenticationFailedException)
            {
                return false;
            }
        }

        private int GetMessageCount()
        {
            ServerResponse resp;

            Utilities.SendResponse(ServerStream, "STAT\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if (resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                throw new CommandNotSupportedException("The STAT command was rejected by the server.");
            }

            String[] resps = resp.Message.Split(' ');
            int Count = 0;
            if (resps.Length == 0 || !int.TryParse(resps[0], out Count))
            {
                throw new ServerResponseInvalidException("The server's response to the STAT command did not contain valid data.");
            }

            return Count;
        }

        private MessageList BuildMessageList()
        {
            MailboxMessage[] Messages = new MailboxMessage[MessageCount];
            for (int i = 0; i < MessageCount; i++)
            {
                Messages[i] = new PopClientMessage(this, i + 1);
            }
            return new MessageList(Messages);
        }

        internal void Delete(int MessageNumber)
        {
            ServerResponse resp;

            Utilities.SendResponse(ServerStream, "DELE " + MessageNumber + "\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if (resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                throw new DeleteMessageFailedException("The server reported an error while trying to delete the message.");
            }
        }

        internal String GetUidl(int MessageNumber)
        {
            ServerResponse resp;

            Utilities.SendResponse(ServerStream, "UIDL " + MessageNumber + "\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if (resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                // TODO: Maybe return an empty string instead of throwing an exception here?
                throw new CommandNotSupportedException("The UIDL command was rejected by the server.");
            }

            String[] resps = resp.Message.Split(' ');
            if (resps.Length != 2)
            {
                throw new ServerResponseInvalidException("The server's response to the UILD command did not contain valid data.");
            }

            // TODO: Enforce the up-to-70-character-limit?
            return resps[1];
        }

        internal long GetSize(int MessageNumber)
        {
            ServerResponse resp;

            Utilities.SendResponse(ServerStream, "LIST " + MessageNumber + "\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if (resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                throw new CommandNotSupportedException("The LIST command was rejected by the server.");
            }

            String[] resps = resp.Message.Split(' ');
            long Size = 0;
            if (resps.Length != 2 || !long.TryParse(resps[1], out Size))
            {
                throw new ServerResponseInvalidException("The server's response to the LIST command did not contain valid data.");
            }

            return Size;
        }

        internal MimeMessage GetMimeMessage(int MessageNumber)
        {
            ServerResponse resp;

            Utilities.SendResponse(ServerStream, "RETR " + MessageNumber + "\r\n");
            resp = Utilities.GetServerResponse(ServerStream);
            if (resp == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }
            if (resp.ResponseLevel != ResponseLevel.OK)
            {
                throw new CommandNotSupportedException("The RETR command was rejected by the server.");
            }
            
            MimeMessage msg = Utilities.ReadMimeMessage(ServerStream);
            if (msg == null)
            {
                throw new ConnectionFailedException("The connection to the server failed.");
            }

            return msg;
        }
    }
}
