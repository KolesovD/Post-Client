namespace JohnGietzen.MailUtilities.Pop
{
    using System;
    using System.Collections.Generic;

    public class MessageList
    {
        private IList<MailboxMessage> Messages;

        public int Count
        {
            get { return Messages.Count; }
        }

        public long Size
        {
            get
            {
                long size = 0;
                foreach (MailboxMessage m in Messages)
                {
                    size += m.Size;
                }
                return size;
            }
        }

        public MailboxMessage this[int messageIndex]
        {
            get
            {
                if (messageIndex < 1 || messageIndex > Messages.Count)
                    throw new ArgumentOutOfRangeException("messageIndex");
                return Messages[messageIndex - 1];
            }
        }

        public MessageList(IList<MailboxMessage> messages)
        {
            Messages = messages;
        }

        public void Process()
        {
            foreach (MailboxMessage m in Messages)
            {
                m.Process();
            }
        }

    }
}