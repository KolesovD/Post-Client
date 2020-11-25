namespace JohnGietzen.MailUtilities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public class MimeMessage
    {
        public MimeNode RootNode
        {
            get { return _RootNode; }
            set { _RootNode = value; }
        }
        private MimeNode _RootNode;

        internal static String FullMessageMatch =
            @"\A(?<header>(?:[^\r\n]+\r\n)*)(?<header_term>\r\n)(?<body>.*)\z";
        internal static String HeadersMatch =
            @"^(?<header_key>[-A-Za-z0-9]+)(?<seperator>:[ \t]*)(?<header_value>([^\r\n]|\r\n[ \t]+)*)(?<terminator>\r\n)";
        internal static String HeaderSeperator =
            "\r\n";
        internal static String KeyValueSeparator =
            @"\A:[ \t]*\z";

        public MimeMessage(Stream messageStream)
        {
            Load(messageStream);
        }

        public MimeMessage(Byte[] messageData)
        {
            Load(messageData);
        }

        public MimeMessage(String messageData)
        {
            Load(messageData);
        }

        public MimeMessage(MimeNode rootNode)
        {
            _RootNode = rootNode;
        }

        public MimeMessage()
        {
            _RootNode = MimeNode.FromData(
                "Content-Type: text/plain; charset=\"iso-8859-1\"" + "\r\n" +
                "Content-Transfer-Encoding: 8bit" + "\r\n" +
                "\r\n");
        }

        public static MimeMessage ReadFrom(String fileName)
        {
            return new MimeMessage(File.ReadAllText(fileName));
        }

        public void SaveTo(String fileName)
        {
            File.WriteAllText(fileName, this.ToString());
        }

        public void Load(Stream messageStream)
        {
            String messageData = "";
            Byte[] buffer = new Byte[1024];
            while (true)
            {
                int count = messageStream.Read(buffer, 0, buffer.Length);
                messageData += Encoding.ASCII.GetString(buffer, 0, count);
                if (count < buffer.Length) break;
            }

            Load(messageData);
        }

        public void Load(Byte[] messageData)
        {
            Load(Encoding.UTF8.GetString(messageData));
        }

        public void Load(String messageData)
        {
            Match m = Regex.Match(messageData, @"\A(?<data>.*)(?<terminator>\r\n.\r\n)\z", RegexOptions.Singleline);
            if (!m.Success)
            {
                Debug.WriteLine("=================================");
                Debug.WriteLine(messageData);
                Debug.WriteLine("=================================");
                throw new InvalidMimeMessageException("The message specified did not end with exactly one period on a single line.");
            }
            _RootNode = MimeNode.FromData(m.Groups["data"].Value);
        }

        public override String ToString()
        {
            return (_RootNode != null ? _RootNode.RawData : "") + "\r\n.\r\n";
        }

        public Collection<MimeLeafNode> GetAllLeafNodes()
        {
            return RootNode.GetAllLeafNodes();
        }
    }
}
