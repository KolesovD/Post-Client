using System;

namespace JohnGietzen.MailUtilities
{
    [global::System.Serializable]
    public class InvalidMimeMessageException : Exception
    {
        public InvalidMimeMessageException() { }
        public InvalidMimeMessageException(String message) : base(message) { }
        public InvalidMimeMessageException(String message, Exception inner) : base(message, inner) { }
        protected InvalidMimeMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
