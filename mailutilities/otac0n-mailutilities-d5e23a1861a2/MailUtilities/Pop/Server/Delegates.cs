using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace JohnGietzen.MailUtilities.Pop.Server
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public Guid ConnectionId
        {
            get;
            set;
        }

        public IPEndPoint RemoteEndpoint
        {
            get;
            set;
        }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public Guid ConnectionId
        {
            get;
            set;
        }
    }

    public class ClientAuthenticatedEventArgs : EventArgs
    {
        public Guid ConnectionId
        {
            get;
            set;
        }

        public String Account
        {
            get;
            set;
        }
    }
}
