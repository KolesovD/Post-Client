namespace JohnGietzen.MailUtilities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;

    public class MimeHeaderCollection : Collection<MimeHeader>
    {
        public MimeHeaderCollection(String rawHeader)
        {
            MatchCollection FullHeaders = Regex.Matches(rawHeader, MimeMessage.HeadersMatch, RegexOptions.Multiline);
            foreach (Match FullHeader in FullHeaders)
            {
                String Key = FullHeader.Groups["header_key"].Value;
                String Value = FullHeader.Groups["header_value"].Value;
                String Seperator = FullHeader.Groups["seperator"].Value;
                this.Add(new MimeHeader(Key, Value, Seperator));
            }
        }

        public Collection<String> GetHeaders(String headerName)
        {
            Collection<String> HeadersList = new Collection<String>();
            foreach (MimeHeader Header in this)
            {
                if (Header.Key.ToUpperInvariant() == headerName.ToUpperInvariant())
                    HeadersList.Add(Header.Value);
            }
            return HeadersList;
        }

        public override String ToString()
        {
            String RawValue = "";
            foreach (MimeHeader Header in this)
            {
                RawValue += Header.ToString() + MimeMessage.HeaderSeperator;
            }
            return RawValue;
        }
    }
}
