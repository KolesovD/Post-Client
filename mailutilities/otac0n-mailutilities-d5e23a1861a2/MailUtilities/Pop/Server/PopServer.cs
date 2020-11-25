namespace JohnGietzen.MailUtilities.Pop.Server
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class PopServer
    {
        public Int32 Port {
            get
            {
                return _Port;
            }
            set
            {
                if (Listener != null &&
                    Listener.ThreadState == ThreadState.Running ||
                    Listener.ThreadState == ThreadState.Background)
                {
                    // TODO: Update the listen thread to restart the Listener with the new port when this is called.
                    throw new InvalidOperationException("The port of the server cannot be set while it is listening.");
                }
                else
                {
                    _Port = value;
                }
            }
        }
        private Int32 _Port = 110;
        
        private Thread Listener;

        private IMailboxProvider MailboxProvider;

        public PopServer(IMailboxProvider mailboxProvider)
        {
            if (mailboxProvider == null)
                throw new ArgumentNullException("mailboxProvider");

            Listener = new Thread(new ThreadStart(Listen));
            Listener.IsBackground = true;

            MailboxProvider = mailboxProvider;
        }

        public void Start()
        {
            Cancel = false;
            Listener.Start();
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public event EventHandler<ClientAuthenticatedEventArgs> ClientAuthenticated;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        internal void RaiseClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (ClientConnected != null)
                ClientConnected(sender, e);
        }

        internal void RaiseClientAuthenticated(object sender, ClientAuthenticatedEventArgs e)
        {
            if (ClientAuthenticated != null)
                ClientAuthenticated(sender, e);
        }

        internal void RaiseClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            if (ClientDisconnected != null)
                ClientDisconnected(sender, e);
        }

        public void Stop()
        {
            Cancel = true;
            Listener.Join(100);
            Listener = new Thread(new ThreadStart(Listen));
            Listener.IsBackground = true;

            foreach (PopServerClientConnection Connection in ClientConnections)
            {
                if (Connection.Active)
                {
                    Connection.Kill();
                }
            }
        }

        public ThreadState Status { get { return Listener.ThreadState; } }

        List<PopServerClientConnection> ClientConnections = new List<PopServerClientConnection>();

        private volatile Boolean Cancel;
        private void Listen()
        {
            TcpListener Incoming = new TcpListener(IPAddress.Any, Port);
            Incoming.Start();
            try
            {
                while (true)
                {
                    while (!Incoming.Pending())
                        if (Cancel) return;
                        else Thread.Sleep(0);
                    TcpClient NewClient = Incoming.AcceptTcpClient();

                    PopServerClientConnection ClientConnection = new PopServerClientConnection(this, NewClient, MailboxProvider);
                    ClientConnections.Add(ClientConnection);
                }
            }
            catch (ThreadAbortException)
            {
            }
            finally
            {
                Incoming.Stop();
            }
        }
    }
}
