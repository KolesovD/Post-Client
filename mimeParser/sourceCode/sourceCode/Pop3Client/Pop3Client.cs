using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Amende.Snorre
{
    public class Pop3Client
    {
        string _userName;
        string _password;
        string _server;
        TcpClient _connection;
        public Pop3Client(string userName, string password, string server)
        {
            _userName = userName;
            _password = password;
            _server = server;
        }
        public bool Connect()
        {
            bool returnValue = false;
            _connection = new TcpClient();
            _connection.Connect(_server, 110);
            if(GetResponse().StartsWith("+OK"))
            {
                SendCommand("USER "+_userName +"\r\n");
                string response = GetResponse();
                if (response.StartsWith("+OK"))
                {
                    SendCommand("PASS " + _password + "\r\n");
                    response = GetResponse();
                    if (response.StartsWith("+OK"))
                        returnValue = true;
                }
            }
            return returnValue;
        }

        private void SendCommand(string p)
        {
            NetworkStream stream = _connection.GetStream();
            System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding();
            byte [] buffer = en.GetBytes(p);
            stream.Write(buffer,0,en.GetByteCount(p));
        }
        private string GetResponse()
        {
            NetworkStream s = _connection.GetStream();
            const int buffersize = 1024;
            byte[] buffer = new byte[buffersize];

            StringBuilder returnValue = new StringBuilder();
            int bytesRead = 0;

            do
            {
                bytesRead = s.Read(buffer, 0, buffersize);
                System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding();
                returnValue.Append(en.GetString(buffer, 0, bytesRead));
            } while (bytesRead == buffersize);

            return returnValue.ToString();
        }

        public Dictionary<int, int> GetMessages()
        {
            Dictionary<int, int>  returnValue = new Dictionary<int, int>();
            SendCommand("LIST\r\n");
            string response = GetResponse();
            if (response.StartsWith("+OK"))
            {
                StringReader reader = new StringReader(response);
                reader.ReadLine(); // take away response line
                string line;
                while (!(line = reader.ReadLine()).Equals("."))
                {
                    string[] parts = line.Split(' ');
                    if (parts.Length == 2)
                    {
                        int id = System.Convert.ToInt32(parts[0]);
                        int size = System.Convert.ToInt32(parts[1]);
                        returnValue[id] = size;
                    }
                }

            }

            return returnValue;
        }

        public string GetMessage(int i)
        {
            StringBuilder returnValue = new StringBuilder();
            SendCommand("RETR "+i+"\r\n");
            string response = GetResponse();
            if (response.StartsWith("+OK"))
            {
                StringReader reader = new StringReader(response);
                reader.ReadLine(); // take away response line
                string line;
                while(!(line = reader.ReadLine()).Equals("."))
                {
                    returnValue.AppendLine(line);
                } 
            }
            return returnValue.ToString();
        }

        public bool DeleteMessage(int i)
        {
            StringBuilder returnValue = new StringBuilder();
            SendCommand("DELE "+i+"\r\n");
            string response = GetResponse();
            return response.StartsWith("+OK");
        }

        public void Disconnect()
        {
            StringBuilder returnValue = new StringBuilder();
            SendCommand("QUIT\r\n");
            string response = GetResponse();
        }
    }
}
