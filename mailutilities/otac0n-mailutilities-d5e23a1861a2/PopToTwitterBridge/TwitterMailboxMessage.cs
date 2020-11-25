namespace TwitterPop
{
    using JohnGietzen.MailUtilities;
    using JohnGietzen.MailUtilities.Pop;
    using System;
    using System.Globalization;

    public class TwitterMailboxMessage : MailboxMessage
    {
        private bool deleted;
        private MimeMessage message;
        private long id;

        public TwitterMailboxMessage(TwitterMessage t, string accountName)
        {
            var date = DateTime.ParseExact(t.created_at, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);

            var node = new MimeLeafNode(
                                   "Message-ID: <" + t.id.ToString() + "@twitter.com>\r\n" +
                                   "From: " + t.user.screen_name + "@twitter.com\r\n" +
                                   "To: " + accountName + "@twitter.com\r\n" +
                                   "Subject: " + t.user.screen_name + ": " + t.text + ".\r\n" +
                                   "Date: " + date.ToString("ddd, d MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture) + " " + date.ToString("zzz", CultureInfo.InvariantCulture).Replace(":", "") + "\r\n" +
                                   "Content-Type: text/html; charset=\"iso-8859-1\"\r\n" +
                                   "Content-Transfer-Encoding: base64\r\n", "")
                               {
                                   Body = "<html><body><img src=\"" + t.user.profile_image_url + "\" /><a href=\"http://twitter.com/" + t.user.screen_name + "\">" + t.user.name + "</a>&nbsp;" + t.text + "</body></html>"
                               };

            this.message = new MimeMessage(node);
            this.id = t.id;
        }

        public override void Delete()
        {
            this.deleted = true;
        }

        public override bool Deleted
        {
            get
            {
                return this.deleted;
            }
        }

        public override MimeMessage MimeMessage
        {
            get
            {
                return this.message;
            }
        }

        public override string UIDL
        {
            get
            {
                return id.ToString();
            }
        }

        public override void Process()
        {
        }

        public override long Size
        {
            get
            {
                return MimeMessage.ToString().Length - 5;
            }
        }
    }
}
