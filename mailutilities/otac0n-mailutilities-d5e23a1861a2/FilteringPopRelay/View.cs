using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JohnGietzen.MailUtilities;
using System.Xml.Linq;
using System.IO;
using JohnGietzen.MailUtilities.Pop.Server;
using JohnGietzen.MailUtilities.Pop.Client;
using JohnGietzen.MailUtilities.Pop;
using System.Threading;

namespace FilteringPopRelay
{
    public partial class View : Form
    {
        RelayMailboxProvider RMbx;
        PopServer Server;

        public View()
        {
            RMbx = new RelayMailboxProvider(FilteringPopRelay.Properties.Settings.Default.MailboxesPath);
            Server = new PopServer(RMbx);

            Server.ClientConnected += new EventHandler<ClientConnectedEventArgs>(Server_ClientConnected);

            InitializeComponent();

            ServerStart_Click(this, null);
            ClientStart_Click(this, null);
        }

        void Server_ClientConnected(object source, ClientConnectedEventArgs e)
        {
            // Client Connected
        }

        private List<AccountDescriptor> LoadAccounts(String accountsPath)
        {
            List<AccountDescriptor> Accounts = new List<AccountDescriptor>();

            // Load the config.
            using (StreamReader reader = new StreamReader(Path.Combine(accountsPath, "accounts.xml")))
            {
                XDocument AccountsXml = XDocument.Load(reader, LoadOptions.None);
                var accounts = from account in AccountsXml.Descendants("account")
                               select new
                               {
                                   Enabled = account.Attribute("enabled").Value,
                                   Username = account.Attribute("username").Value,
                                   Caches = account.Elements("cache")
                               };

                foreach (var account in accounts)
                {
                    // WARNING! WE MUST CHANGE THE PATH TO EXCLUDE \ / ? : etc...
                    AccountDescriptor Descr = new AccountDescriptor(account.Username, Path.Combine(accountsPath, account.Username));

                    var caches = from cache in account.Caches
                                 select new
                                 {
                                     Server = cache.Element("server").Value,
                                     Port = cache.Element("port").Value,
                                     UseSSL = cache.Attribute("usessl").Value.ToUpperInvariant(),
                                     AccountName = cache.Element("accountname").Value,
                                     Password = cache.Element("password").Value,
                                     EmailAddress = cache.Element("emailaddress").Value,
                                     Filters = cache.Element("filters").Elements("filter"),
                                     MessageRemoval = cache.Element("remove").Value.ToUpperInvariant(),
                                 };

                    foreach (var cache in caches)
                    {
                        Boolean UseSSL = cache.UseSSL == "TRUE";
                        int Port = int.Parse(cache.Port);
                        MessageRemoval remove = MessageRemoval.None;

                        if (cache.MessageRemoval == "SPAM")
                        {
                            remove = MessageRemoval.Spammail;
                        }
                        if (cache.MessageRemoval == "BADMAIL")
                        {
                            remove = MessageRemoval.Badmail;
                        }
                        if (cache.MessageRemoval == "SPAMANDBADMAIL")
                        {
                            remove = MessageRemoval.SpamAndBadmailOnly;
                        }
                        if (cache.MessageRemoval == "ALL")
                        {
                            remove = MessageRemoval.All;
                        }

                        CacheDescriptor CacheDescr = new CacheDescriptor(cache.Server, Port, UseSSL, cache.AccountName, cache.Password, cache.EmailAddress, remove);
                        foreach (var Filter in cache.Filters)
                        {
                            CacheDescr.Filters.Add(Filter.Value);
                        }
                        Descr.Caches.Add(CacheDescr);
                    }

                    Accounts.Add(Descr);
                }
            }


            return Accounts;
        }

        private void StatusPollingTimer_Tick(object sender, EventArgs e)
        {
            ServerStatus.Text = ((Server.Status & ThreadState.Unstarted) == ThreadState.Unstarted) ? "Unstarted" : "Running";
            ClientStatus.Text = RMbx.PollersActive + " / " + RMbx.PollerCount + " pollers active";

            ClientStart.Enabled = (RMbx.PollerCount == 0);
            ClientStop.Enabled = !ClientStart.Enabled;

            ServerStart.Enabled = ((Server.Status & ThreadState.Unstarted) == ThreadState.Unstarted);
            ServerStop.Enabled = !ServerStart.Enabled;
        }

        private void ServerStart_Click(object sender, EventArgs e)
        {
            ServerStart.Enabled = false;
            Server.Start();
        }

        private void ServerStop_Click(object sender, EventArgs e)
        {
            ServerStop.Enabled = false;
            Server.Stop();
        }

        private void ClientStart_Click(object sender, EventArgs e)
        {
            ClientStart.Enabled = false;
            List<AccountDescriptor> Accounts = LoadAccounts(@"C:\Mailboxes\");
            RMbx.Start(Accounts);
        }

        private void ClientStop_Click(object sender, EventArgs e)
        {
            ClientStop.Enabled = false;
            RMbx.Stop();
        }

        private void View_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
