
using System;
namespace JohnGietzen.MailUtilities
{
    struct SpamFilterResult
    {
        public Boolean IsSpam;
        public String Reason;
    }

    abstract class SpamFlter
    {
        public abstract SpamFilterResult FilterSpam(MimeMessage message);
        public abstract void FeedbackResult(MimeMessage message, SpamFilterResult result);
        public abstract void Save();
    }
}
