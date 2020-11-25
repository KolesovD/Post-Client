namespace TwitterPop
{
    using JohnGietzen.MailUtilities.Pop;

    internal class TwitterMailboxProvider : IMailboxProvider
    {
        public bool PasswordsKnown
        {
            get
            {
                return false;
            }
        }

        public IMailboxAccount GetAccount(string accountName)
        {
            return new TwitterMailboxAccount(accountName);
        }
    }
}
