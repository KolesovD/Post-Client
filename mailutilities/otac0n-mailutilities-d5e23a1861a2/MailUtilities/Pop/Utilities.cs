namespace JohnGietzen.MailUtilities.Pop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal enum ResponseLevel
    {
        OK,
        ERR
    }

    internal class ServerResponse
    {
        public ResponseLevel ResponseLevel;
        public String Message;

        public ServerResponse(ResponseLevel responseLevel, String message)
        {
            ResponseLevel = responseLevel;
            Message = message;
        }
    }

    internal class ClientRequest
    {
        public String Command;
        public String Parameters;

        public ClientRequest(String message)
        {
            Match mm = Regex.Match(message, @"(?<command>\w+)(\s(?<parameters>.+))?\r\n");
            Command = mm.Groups["command"].Value;
            Parameters = mm.Groups["parameters"].Value;
        }
    }

    internal class Utilities
    {
        private Utilities() { }

        public static ServerResponse GetServerResponse(Stream stream)
        {
            string Command = GetPopCommand(stream);
            if (String.IsNullOrEmpty(Command))
                return null;
            Match m = Regex.Match(Command, @"(?<code>\+OK|-ERR)(?:\s(?<info>[^\r\n]+))?\r\n");

            if (m == null)
                return null;

            return new ServerResponse(
                (m.Groups["code"].Value == "+OK" ? ResponseLevel.OK : ResponseLevel.ERR),
                m.Groups["info"].Value
            );
        }

        public static ClientRequest GetClientRequest(Stream stream)
        {
            string Command = GetPopCommand(stream);
            if (Command == null)
                return null;
            return new ClientRequest(Command);
        }

        public static void SendResponse(Stream stream, String response)
        {
            byte[] buffer = null;
            buffer = Encoding.ASCII.GetBytes(response);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static string GetPopCommand(Stream stream)
        {
            return ReadWithTerminator(stream, Encoding.ASCII.GetBytes("\r\n"));
        }

        internal static MimeMessage ReadMimeMessage(Stream stream)
        {
            return new MimeMessage(ReadWithTerminator(stream, Encoding.ASCII.GetBytes("\r\n.\r\n")));
        }

        private static String ReadWithTerminator(Stream stream, byte[] terminator)
        {
            List<Byte> MessageBytes = new List<Byte>();
            int input = 0;

            NetworkStream n = stream as NetworkStream;

            while (true)
            {
                if (n != null)
                {
                    // Here, we need to implement a timeout...
                    while (!n.DataAvailable)
                        Thread.Sleep(0);
                }

                try
                {
                    input = stream.ReadByte();
                }
                catch (IOException)
                {
                    input = -2;
                }

                // If an IOException was caught, or the end of the stream was reached, discard the message.
                if (input == -2 || input == -1)
                {
                    MessageBytes.Clear();
                    break;
                }

                MessageBytes.Add((byte)input);
                if (MessageBytes.Count >= terminator.Length)
                {
                    Boolean terminated = true;
                    for (int i = terminator.Length-1, j = MessageBytes.Count-1; i >= 0; i--, j--)
                    {
                        if (terminator[i] != MessageBytes[j])
                        {
                            terminated = false;
                            break;
                        }
                    }
                    if (terminated) break;
                }
            }

            if (MessageBytes.Count == 0)
            {
                return null;
            }
            else
            {
                return Encoding.UTF8.GetString(MessageBytes.ToArray());
            }
        }
    }
}
