namespace TwitterPop
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using JohnGietzen.MailUtilities;
    using JohnGietzen.MailUtilities.Pop;
    using Newtonsoft.Json;

    class TwitterMailboxAccount : IMailboxAccount
    {
        public TwitterMailboxAccount(string username)
        {
            this.Name = username;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Password
        {
            get
            {
                throw new PasswordNotAvailableException();
            }
        }

        private string password;

        private List<TwitterMessage> tweets = null;

        public bool ConfirmPassword(string password)
        {
            this.password = password;

            var request = (HttpWebRequest)WebRequest.Create("http://api.twitter.com/statuses/home_timeline.json");
            request.Credentials = new NetworkCredential(this.Name, this.password);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                string json;

                using (var s = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(s, Encoding.UTF8))
                    {
                        json = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }

                this.tweets = JsonConvert.DeserializeObject<List<TwitterMessage>>(json);

                return true;
            }
            catch (WebException ex)
            {
                return false;
            }
        }

        public MessageList MessageList
        {
            get
            {
                if (this.tweets != null)
                {
                    var messages = from t in this.tweets
                                   select (MailboxMessage)new TwitterMailboxMessage(t, this.Name);

                    return new MessageList(messages.ToList());
                }
                else
                {
                    return new MessageList(new List<MailboxMessage>());
                }
            }
        }
    }
}
