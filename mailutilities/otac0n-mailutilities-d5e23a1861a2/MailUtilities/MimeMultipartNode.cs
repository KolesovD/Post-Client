namespace JohnGietzen.MailUtilities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    public class MimeMultipartNode : MimeNode
    {
        public String Preamble
        {
            get { return _Preamble; }
            set {
                // TODO: Preamble must be empty or end with "\r\n"
                _Preamble = value;
            }
        }
        private String _Preamble;

        public String Footer
        {
            get { return _Footer; }
            set {
                // TODO: Footer must be empty or begin with "\r\n"
                _Footer = value;
            }
        }
        private String _Footer;

        public String BaseBoundary
        {
            get { return _BaseBoundary; }
            set { _BaseBoundary = value; }
        }
        private String _BaseBoundary = "";
        

        public Collection<MimeNode> Parts
        {
            get { return _Parts; }
        }
        private Collection<MimeNode> _Parts = new Collection<MimeNode>();

        public MimeMultipartNode(String header, String body)
        {
            _Headers = new MimeHeaderCollection(header);
            Collection<String> ContentTypeList = _Headers.GetHeaders("Content-Type");
            if (ContentTypeList.Count != 1)
            {
                throw new InvalidOperationException("The headers specified did not contain exactly one Content-Type header.");
            }
            String ContentType = ContentTypeList[0];

            if (ContentType.IndexOf("multipart", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidOperationException("The headers specified did not contain a multipart Content-Type.");
            }   

            Match BoundaryMatch = Regex.Match(ContentType, @"boundary=""?(?<boundary>[^"";\r\n]+)""?;?", RegexOptions.IgnoreCase);
            _BaseBoundary = BoundaryMatch.Groups["boundary"].Value;
            String Boundary = BoundaryMatch.Groups["boundary"].Value
                .Replace("\\", @"\\")
                .Replace("[", @"\[").Replace("^", @"\^").Replace("$", @"\$").Replace(".", @"\.")
                .Replace("|", @"\|").Replace("?", @"\?").Replace("*", @"\*").Replace("+", @"\+")
                .Replace("(", @"\(").Replace(")", @"\)").Replace("{", @"\{").Replace("}", @"\}");

            String FullMatchRegex = @"(?<preamble>(?:(?:[^\r\n]*)\r\n)*?)(?<first_boundary>(?<=(?:\r\n|\A))--" + Boundary +
                                    @"\r\n)(?<message_parts>.*)(?<last_boundary>\r\n--" + Boundary +
                                    @"--(?=(?:\r\n|\z)))(?<footer>.*)";

            Match FullMatch = Regex.Match(body, FullMatchRegex, RegexOptions.Singleline);

            if (!FullMatch.Success)
            {
                Debug.WriteLine(FullMatchRegex);
                Debug.WriteLine("".PadLeft(FullMatchRegex.Length, '='));
                Debug.Write(body);
                throw new InvalidOperationException("The body specified did not follow the format of a multipart message on boundary \"" + _BaseBoundary + "\".");
            }
            
            _Preamble = FullMatch.Groups["preamble"].Value;
            _Footer = FullMatch.Groups["footer"].Value;
            String[] SingleParts = Regex.Split(FullMatch.Groups["message_parts"].Value, @"\r\n--" + Boundary + @"\r\n");
            foreach (String Part in SingleParts)
            {
                _Parts.Add(MimeNode.FromData(Part));
            }
        }

        public override MimeHeaderCollection Headers
        {
            get { return _Headers; }
        }
        private MimeHeaderCollection _Headers;

        public override String RawBody
        {
            get {
                if (Parts.Count == 0)
                {
                    throw new InvalidOperationException("A multipart node with no parts is not valid.");
                }

                String[] PartsText = new String[Parts.Count];
                String Boundary = _BaseBoundary;

                for (int i = 0; i < PartsText.Length; i++)
                {
                    PartsText[i] = Parts[i].RawData;
                }

                Random rng = new Random();

                Boolean BoundaryGood = false;
                while (!BoundaryGood)
	            {
                    BoundaryGood = true;
                    foreach (String Part in PartsText)
                    {
                        if (Part.IndexOf(Boundary, StringComparison.Ordinal) >= 0)
                        {
                            BoundaryGood = false;
                            Boundary += rng.Next(10);
                            break;
                        }
                    }
                }

                String FullText = "";
                foreach (String Part in PartsText)
                {
                    if (String.IsNullOrEmpty(FullText))
                        FullText = _Preamble + "--" + Boundary + "\r\n" + Part;
                    else
                        FullText += "\r\n--" + Boundary + "\r\n" + Part;
                }
                FullText += "\r\n--" + Boundary + "--" + _Footer;

                return FullText;
            }
        }

        public override String RawHeader
        {
            get { return _Headers.ToString(); }
        }
    }
}
