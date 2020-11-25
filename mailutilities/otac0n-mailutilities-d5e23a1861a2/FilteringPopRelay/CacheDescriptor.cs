using System;
using System.Collections.Generic;

namespace JohnGietzen.MailUtilities
{
    [Flags]
    public enum MessageRemoval
    {
        Badmail = 0x01,
        Spammail = 0x02,
        Realmail = 0x04,

        None = 0x00,
        SpamAndBadmailOnly = 0x03,
        All = 0x07,
    }

    [Serializable]
    public class CacheDescriptor
    {
        public String Server { get { return _Server; } }
        private String _Server;

        public Int32 Port { get { return _Port; } }
        private Int32 _Port;

        public String AccountName { get { return _AccountName; } }
        private String _AccountName;

        public String Password { get { return _Password; } }
        private String _Password;

        public String EmailAddress { get { return _EmailAddress; } }
        private String _EmailAddress;

        public Boolean UseSSL { get { return _UseSSL; } }
        private Boolean _UseSSL;

        public MessageRemoval MessageRemoval { get { return _MessageRemoval; } }
        private MessageRemoval _MessageRemoval = MessageRemoval.None;

        public List<String> Filters = new List<string>();

        public CacheDescriptor(String server, int port, Boolean useSSL, String accountName, String password, string emailAddress, MessageRemoval messageRemoval)
        {
            _Server = server;
            _Password = password;
            _Port = port;
            _UseSSL = useSSL;
            _AccountName = accountName;
            _EmailAddress = emailAddress;
            _MessageRemoval = messageRemoval;
        }
    }
}
