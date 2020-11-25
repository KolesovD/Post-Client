
using System;
using System.Runtime.Serialization;

namespace JohnGietzen.MailUtilities.Pop.Client
{
    [global::System.Serializable]
    public class PopClientException : Exception
    {
        public PopClientException() { }
        public PopClientException(String message) : base(message) { }
        public PopClientException(String message, Exception inner) : base(message, inner) { }
        protected PopClientException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class ConnectionFailedException : PopClientException
    {
        public ConnectionFailedException() { }
        public ConnectionFailedException(String message) : base(message) { }
        public ConnectionFailedException(String message, Exception inner) : base(message, inner) { }
        protected ConnectionFailedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class AuthenticationFailedException : PopClientException
    {
        public AuthenticationFailedException() { }
        public AuthenticationFailedException(String message) : base(message) { }
        public AuthenticationFailedException(String message, Exception inner) : base(message, inner) { }
        protected AuthenticationFailedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class UserNameRejectedException : AuthenticationFailedException
    {
        public UserNameRejectedException() { }
        public UserNameRejectedException(String message) : base(message) { }
        public UserNameRejectedException(String message, Exception inner) : base(message, inner) { }
        protected UserNameRejectedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class PasswordRejectedException : AuthenticationFailedException
    {
        public PasswordRejectedException() { }
        public PasswordRejectedException(String message) : base(message) { }
        public PasswordRejectedException(String message, Exception inner) : base(message, inner) { }
        protected PasswordRejectedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class MailboxLockFailedException : AuthenticationFailedException
    {
        public MailboxLockFailedException() { }
        public MailboxLockFailedException(String message) : base(message) { }
        public MailboxLockFailedException(String message, Exception inner) : base(message, inner) { }
        protected MailboxLockFailedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class DeleteMessageFailedException : PopClientException
    {
        public DeleteMessageFailedException() { }
        public DeleteMessageFailedException(String message) : base(message) { }
        public DeleteMessageFailedException(String message, Exception inner) : base(message, inner) { }
        protected DeleteMessageFailedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class CommandNotSupportedException : PopClientException
    {
        public CommandNotSupportedException() { }
        public CommandNotSupportedException(String message) : base(message) { }
        public CommandNotSupportedException(String message, Exception inner) : base(message, inner) { }
        protected CommandNotSupportedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class ServerResponseInvalidException : PopClientException
    {
        public ServerResponseInvalidException() { }
        public ServerResponseInvalidException(String message) : base(message) { }
        public ServerResponseInvalidException(String message, Exception inner) : base(message, inner) { }
        protected ServerResponseInvalidException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [global::System.Serializable]
    public class UncleanDisconnectException : PopClientException
    {
        public UncleanDisconnectException() { }
        public UncleanDisconnectException(String message) : base(message) { }
        public UncleanDisconnectException(String message, Exception inner) : base(message, inner) { }
        protected UncleanDisconnectException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
}