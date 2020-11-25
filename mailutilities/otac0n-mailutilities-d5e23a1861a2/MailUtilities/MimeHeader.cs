namespace JohnGietzen.MailUtilities
{
    using System;
    using System.Text.RegularExpressions;

    public class MimeHeader
    {
        private string separator = ": ";
        
        public MimeHeader(string key, string value, string separator)
        {
            this.Key = key;
            this.Value = value;
            this.Separator = separator;
        }

        public string Key
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            set;
        }

        public string Separator
        {
            get
            {
                return this.separator;
            }

            set
            {
                if (!Regex.IsMatch(value, MimeMessage.KeyValueSeparator))
                {
                    throw new InvalidOperationException("A seperator must be a colon, followed by zero or more spaces or tabs.");
                }
                
                this.separator = value;
            }
        }

        /// <summary>
        /// Turns the mime header into it's string representation.
        /// </summary>
        /// <returns>The string representation of the header.</returns>
        public override string ToString()
        {
            return this.Key + this.separator + this.Value;
        }
    }
}
