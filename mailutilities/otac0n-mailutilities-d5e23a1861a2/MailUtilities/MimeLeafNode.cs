namespace JohnGietzen.MailUtilities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Text.RegularExpressions;

    public class MimeLeafNode : MimeNode
    {
        public MimeLeafNode(String header, String body)
        {
            _RawBody = body;
            _Headers = new MimeHeaderCollection(header);
        }

        public String Body
        {
            get
            {
                switch (ContentTransferEncoding)
                {
                    case "7bit":
                    case "8bit":
                    case "binary":
                        return RawBody;
                    case "quoted-printable":
                        return RawBody;
                    case "base64":
                        return Encoding.UTF8.GetString(System.Convert.FromBase64String(RawBody));
                    default:
                        return RawBody;
                }
            }
            set
            {
                switch (ContentTransferEncoding)
                {
                    case "7bit":
                    case "8bit":
                    case "binary":
                        _RawBody = value;
                        break;
                    case "quoted-printable":
                        _RawBody = value;
                        break;
                    case "base64":
                        _RawBody = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                        break;
                    default:
                        _RawBody = value;
                        break;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public String ContentType
        {
            get
            {
                Collection<String> ContentType = _Headers.GetHeaders("Content-Type");
                if(ContentType.Count == 0)
                    return "text/plain";

                Match Content = Regex.Match(ContentType[0], "^[-a-zA-Z1-9]+(/[-a-zA-Z1-9]+)+");
                if (!Content.Success)
                    return "text/plain";

                return Content.Value.ToLowerInvariant();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public String ContentTransferEncoding
        {
            get
            {
                Collection<String> TransEncoding = _Headers.GetHeaders("Content-Transfer-Encoding");
                if (TransEncoding.Count == 0)
                {
                    return "8bit";
                }

                return TransEncoding[0].ToLowerInvariant();
            }
        }

        public override MimeHeaderCollection Headers
        {
            get { return _Headers; }
        }
        private MimeHeaderCollection _Headers;

        public override String RawBody
        {
            get { return _RawBody; }
        }
        private String _RawBody;

        public override String RawHeader
        {
            get { return _Headers.ToString(); }
        }
    }
}
