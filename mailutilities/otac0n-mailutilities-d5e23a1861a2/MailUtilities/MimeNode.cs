namespace JohnGietzen.MailUtilities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;

    public abstract class MimeNode
    {
        public String RawData
        {
            get { return RawHeader + MimeMessage.HeaderSeperator + RawBody; }
        }

        public abstract String RawBody { get; }
        public abstract String RawHeader { get; }
        public abstract MimeHeaderCollection Headers { get; }

        public override String ToString()
        {
            return this.RawData;
        }

        public static MimeNode FromData(String mimeData)
        {
            Match FullMessage = Regex.Match(mimeData, MimeMessage.FullMessageMatch, RegexOptions.Singleline);
            if (!FullMessage.Success)
            {
                throw new InvalidMimeMessageException("The message specified did not follow the format of a standard message.\nA message must consist of zero or more lines of headers, followed by a blank line, followed by zero or more lines in the body.");
            }
            String Header = FullMessage.Groups["header"].Value;
            String Body = FullMessage.Groups["body"].Value;

            MimeHeaderCollection Headers = new MimeHeaderCollection(Header);
            Collection<String> ContentTypeList = Headers.GetHeaders("Content-type");

            if (ContentTypeList.Count == 1 && ContentTypeList[0].IndexOf("multipart", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new MimeMultipartNode(Header, Body);
            }
            else
            {
                return new MimeLeafNode(Header, Body);
            }
        }

        public Collection<MimeLeafNode> GetAllLeafNodes()
        {
            Collection<MimeLeafNode> Nodes = new Collection<MimeLeafNode>();
            if (this is MimeLeafNode)
            {
                Nodes.Add(this as MimeLeafNode);
            }
            else
            {
                MimeMultipartNode Node = (MimeMultipartNode)this;
                foreach (MimeNode SubNode in Node.Parts)
                {
                    foreach (MimeLeafNode LeafNode in SubNode.GetAllLeafNodes())
                    {
                        Nodes.Add(LeafNode);
                    }
                }
            }
            return Nodes;
        }
    }
}
