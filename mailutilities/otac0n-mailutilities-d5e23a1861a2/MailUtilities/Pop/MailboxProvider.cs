namespace JohnGietzen.MailUtilities.Pop
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidMailboxException : Exception {
        public InvalidMailboxException()
            : base() { }
        protected InvalidMailboxException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        public InvalidMailboxException(String message)
            : base(message) { }
        public InvalidMailboxException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Thrown when an attempt is made to retrieve the password from an account for which that is not available.
    /// </summary>
    [global::System.Serializable]
    public class PasswordNotAvailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the PasswordNotAvailableException class.
        /// </summary>
        public PasswordNotAvailableException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordNotAvailableException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PasswordNotAvailableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordNotAvailableException class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public PasswordNotAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PasswordNotAvailableException class with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        /// <exception cref="System.ArgumentNullException">The info parameter is null.</exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">The class name is null or System.Exception.HResult is zero (0).</exception>
        protected PasswordNotAvailableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    public interface IMailboxAccount
    {
        String Name { get; }

        String Password { get; }
        bool ConfirmPassword(string password);

        MessageList MessageList { get; }
    }

    public interface IMailboxProvider
    {
        bool PasswordsKnown { get; }
        IMailboxAccount GetAccount(String accountName);
    }

    public abstract class MailboxMessage
    {
        public abstract void Delete();
        public abstract Boolean Deleted { get; }
        public abstract MimeMessage MimeMessage { get; }
        public abstract String UIDL { get; }
        public abstract void Process();
        public abstract Int64 Size { get; }
    }
}
