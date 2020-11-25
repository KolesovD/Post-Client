using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using JohnGietzen.MailUtilities;
using JohnGietzen.MailUtilities.Pop;
using JohnGietzen.MailUtilities.Pop.Client;

namespace FilteringPopRelay
{
    public class RelayMailboxAccount : IMailboxAccount
    {
        public String Name
        {
            get { return _MailboxName; }
        }
        private String _MailboxName;

        public String Password
        {
            get { return _Password; }
        }
        private String _Password;

        public bool ConfirmPassword(string password)
        {
            return this.Password == password;
        }

        private string MailboxPath;

        public RelayMailboxAccount(string mailboxPath)
        {
            String PasswordFile = ""; 
            
            if (!Directory.Exists(mailboxPath))
                throw new InvalidMailboxException();

            mailboxPath = Path.GetFullPath(mailboxPath);
            while(mailboxPath.EndsWith("\\"))
                mailboxPath = mailboxPath.Substring(0, mailboxPath.Length - 1);
            _MailboxName = Path.GetFileName(mailboxPath);

            MailboxPath = mailboxPath;

            PasswordFile = Path.Combine(mailboxPath, "password");

            if (!File.Exists(PasswordFile))
                throw new InvalidMailboxException();

            _Password = Encoding.UTF8.GetString(File.ReadAllBytes(PasswordFile));
        }

        public MessageList MessageList
        {
            get
            {
                string[] Files = Directory.GetFiles(MailboxPath, "*.eml", SearchOption.AllDirectories);
                MailboxMessage[] Messages = new MailboxMessage[Files.Length];
                for (int i = 0; i < Files.Length; i++)
                {
                    Messages[i] = new SimpleMailboxMessage(Files[i]);
                }
                return new MessageList(Messages);
            }
        }
    }

    public class SimpleMailboxMessage : MailboxMessage
    {

        public override void Delete()
        {
            _Delete = true;
        }
        private Boolean _Delete = false;

        public override MimeMessage MimeMessage
        {
            get
            {
                if(_MimeMessage == null)
                    _MimeMessage = MimeMessage.ReadFrom(FileName);
                return _MimeMessage;
            }
        }
        private MimeMessage _MimeMessage;

        public override long Size
        {
            get { return (new FileInfo(FileName)).Length; }
        }

        public override string UIDL
        {
            get
            {
                MD5 md5 = MD5.Create();
                byte[] uidl = md5.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(FileName)));
                return Convert.ToBase64String(uidl);
            }
        }

        private string FileName;

        public override void Process()
        {
            if (_Delete)
            {
                //File.Delete(Filename);
                File.Move(FileName, FileName + ".deleted");
            }
        }

        public SimpleMailboxMessage(string fileName)
        {
            FileName = Path.GetFullPath(fileName);
        }

        public override Boolean Deleted
        {
            get { return _Delete; }
        }
    }

    public class RelayMailboxProvider : IMailboxProvider
    {
        private string MailstorePath = "";

        public bool PasswordsKnown
        {
            get
            {
                return true;
            }
        }

        public IMailboxAccount GetAccount(string accountName)
        {
            string AccountPath = accountName;
            foreach (char Invalid in Path.GetInvalidFileNameChars())
            {
                AccountPath = AccountPath.Replace(Invalid, '_');
            }

            string MailboxPath = Path.Combine(
                                    MailstorePath,
                                    AccountPath
                                 );

            try
            {
                return new RelayMailboxAccount(MailboxPath);
            }
            catch (InvalidMailboxException)
            {
                return null;
            }
        }

        public RelayMailboxProvider(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();

            MailstorePath = Path.GetFullPath(path);
        }

        private List<Thread> PollThreads = new List<Thread>();

        public void Start(List<AccountDescriptor> accounts)
        {
            if (accounts == null)
                throw new ArgumentNullException("accounts");

            foreach (AccountDescriptor Account in accounts)
            {
                Thread PollThread = new Thread(new ParameterizedThreadStart(Poll));
                PollThread.IsBackground = true;
                PollThread.Start(Account);
                PollThreads.Add(PollThread);
            }
        }

        public void Stop()
        {
            foreach (Thread PollThread in PollThreads)
            {
                PollThread.Abort();
                PollThread.Join();
            }
            PollThreads.Clear();
        }

        public int PollerCount
        {
            get { return PollThreads.Count; }
        }

        public int PollersActive
        {
            get
            {
                int Active = 0;
                foreach (Thread PollThread in PollThreads)
                {
                    Active += (PollThread.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin
                        ? 0 
                        : 1;
                }
                return Active;
            }
        }

        private void Poll(object account)
        {
            try
            {
                AccountDescriptor Account = (AccountDescriptor)account;

                Directory.CreateDirectory(Account.WorkingDir);

                // Logger Log = new FileLogger(Path.Combine(Account.WorkingDir, "log.txt"));

                Dictionary<String, SpamFlter> Filters = new Dictionary<String, SpamFlter>();
                Filters.Add("spamhaus", new SpamhausFilter());
                //Filters.Add("stat", new StatFilter(Path.Combine(Account.WorkingDir, "dictionary.xml"), Path.Combine(MailstorePath, "leet.xml")));

                PopClient Client = new PopClient();

                while (true)
                {
                    foreach (CacheDescriptor Cache in Account.Caches)
                    {
                        //Log.Log(LogSeverity.Info, "Poller", "Accessing " + Cache.EmailAddress);
                        try
                        {
                            //Log.Log(LogSeverity.Info, "Poller", "Connecting to " + Cache.Server + ":" + Cache.Port + " (Using SSL: " + (Cache.UseSSL ? "yes" : "no") + ")");
                            Client.Connect(Cache.Server, Cache.Port, Cache.UseSSL);
                            //Log.Log(LogSeverity.Info, "Poller", "Connected");
                        }
                        catch (System.Net.Sockets.SocketException)
                        {
                            //Log.Log(LogSeverity.Warning, "Poller", "Connect failed (socket exception)");
                            continue;
                        }
                        catch (PopClientException)
                        {
                            //Log.Log(LogSeverity.Warning, "Poller", "Connect failed (pop client exception)");
                            continue;
                        }

                        try
                        {
                            //Log.Log(LogSeverity.Info, "Poller", "Authenticating as " + Cache.AccountName);
                            Client.Authenticate(Cache.AccountName, Cache.Password);
                        }
                        catch (MailboxLockFailedException)
                        {
                            //Log.Log(LogSeverity.Warning, "Poller", "Mailbox lock failed");
                            Client.Disconnect(false);
                            continue;
                        }
                        catch (AuthenticationFailedException)
                        {
                            //Log.Log(LogSeverity.Warning, "Poller", "Authentication failed");
                            Client.Disconnect(false);
                            continue;
                        }
                        catch (PopClientException)
                        {
                            //Log.Log(LogSeverity.Warning, "Poller", "General client exception");
                            Client.Disconnect(false);
                            continue;
                        }

                        MessageList Messages = Client.Messages;
                        if (Messages.Count > 0)
                        {
                            //Log.Log(LogSeverity.Info, "Poller", "Retrieving " + Messages.Count + " messages (" + Messages.Size + " bytes)");
                            for (int i = 1; i <= Messages.Count; i++)
                            {
                                // Generate a filename based on the email's unique ID,
                                // the active account, and the recipient's login name
                                string UIDL = Messages[i].UIDL;
                                string MessageFile = Cache.AccountName + "#" + UIDL + ".eml";

                                // Replace any invalid filename characters with underscores
                                foreach (char Invalid in Path.GetInvalidFileNameChars())
                                {
                                    MessageFile = MessageFile.Replace(Invalid, '_');
                                }

                                // Create the working directory if it doesn't exist
                                Directory.CreateDirectory(Account.WorkingDir);

                                // Append the absolute working path to the filename
                                MessageFile = Path.Combine(Account.WorkingDir, MessageFile);

                                // If we already have the message saved to file, skip to
                                // the next one.
                                if (File.Exists(MessageFile) ||
                                    File.Exists(MessageFile + ".deleted") ||
                                    File.Exists(MessageFile + ".spam"))
                                {
                                    //Log.Log(LogSeverity.Info, "Poller", "Message " + i + " already downloaded, skipping");
                                    continue;
                                }

                                // Download the message into our cache.
                                MimeMessage Message;
                                try
                                {
                                    Message = Messages[i].MimeMessage;
                                }
                                catch (InvalidOperationException)
                                {
                                    Message = null;
                                }

                                // Run the message through the account's filters
                                if (Message != null)
                                {
                                    foreach (String Filter in Cache.Filters)
                                    {
                                        if (!Filters.ContainsKey(Filter.ToLower()))
                                            continue;
                                        SpamFilterResult res = Filters[Filter].FilterSpam(Message);
                                        if (res.IsSpam)
                                        {
                                            //Log.Log(LogSeverity.Info, "Poller", "Message " + i + " marked as spam by \"" + Filter + "\" filter [" + res.Reason + "]");

                                            // Feedback the spam result to the filters
                                            foreach (String FilterName in Filters.Keys)
                                            {
                                                Filters[FilterName].FeedbackResult(Message, res);
                                            }

                                            foreach(MimeHeader Header in Message.RootNode.Headers)
                                            {
                                                if (Header.Key.ToUpperInvariant() == "SUBJECT")
                                                {
                                                    Header.Value = "Your son thinks this is spam: " + Header.Value;
                                                }
                                            }
                                            Message.SaveTo(Path.Combine(Account.WorkingDir, MessageFile));
                                            Message = null;

                                            if ((Cache.MessageRemoval & MessageRemoval.Spammail) != MessageRemoval.None)
                                            {
                                                Messages[i].Delete();
                                            }

                                            break;
                                        }
                                    }
                                    if (Message != null)
                                    {
                                        Message.SaveTo(Path.Combine(Account.WorkingDir, MessageFile));
                                        //Log.Log(LogSeverity.Info, "Poller", "Message " + i + " saved to \"" + MessageFile + "\"");

                                        // Feedback the non-spam result to the filters
                                        SpamFilterResult res = new SpamFilterResult();
                                        res.IsSpam = false;
                                        foreach (String FilterName in Filters.Keys)
                                        {
                                            Filters[FilterName].FeedbackResult(Message, res);
                                        }
                                        Message = null;

                                        if ((Cache.MessageRemoval & MessageRemoval.Realmail) != MessageRemoval.None)
                                        {
                                           Messages[i].Delete();
                                        }
                                    }
                                }
                                else
                                {
                                    //Log.Log(LogSeverity.Info, "Poller", "Message " + i + " corrupted or otherwise unreadable, removing from server");

                                    if ((Cache.MessageRemoval & MessageRemoval.Badmail) != MessageRemoval.None)
                                    {
                                        Messages[i].Delete();
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Log.Log(LogSeverity.Info, "Poller", "No messages to retrieve");
                        }

                        //Log.Log(LogSeverity.Info, "Poller", "Disconnecting, Committing Changes");
                        Client.Disconnect(true);
                    }

                    // Save our filters
                    foreach (String FilterName in Filters.Keys)
                    {
                        Filters[FilterName].Save();
                    }

                    // Wait five minutes
                    //Log.Log(LogSeverity.Info, "Poller", "Done, waiting 5 minutes");
                    Thread.Sleep(new TimeSpan(0, 5, 0));
                }
            }
            catch (ThreadAbortException)
            {
            }
        }
    }
}
