using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;

namespace Amende.Snorre
{
    class Program
    {
        static void Main(string[] args)
        {

//            TestPop3();
 //           return;
            string[] files = System.IO.Directory.GetFiles(@"C:\temp\testmsgs", "*.msg");
            foreach (string file in files)
            {
                StreamReader r = new StreamReader(file);
                StringReader reader = new StringReader(r.ReadToEnd());

                System.Console.WriteLine(file);
                MailMessage message = MailMessageMimeParser.ParseMessage(reader);
                if (message.Headers["subject"] != null)
                    System.Console.WriteLine(message.Headers["subject"].ToString());
                else
                    System.Console.WriteLine("missing subject");

                SmtpClient c = new SmtpClient("win2003std");
                message.To.Clear();
                message.To.Add("snorre@garmann.biz");
                message.CC.Clear();
                message.Bcc.Clear();
                int views = 0;
                foreach (AlternateView view in message.AlternateViews)
                {
                    byte[] buffer = new byte[view.ContentStream.Length];
                    view.ContentStream.Read(buffer, 0, (int)view.ContentStream.Length);
                    view.ContentStream.Seek(0, SeekOrigin.Begin);
                    views++;
                }
                c.Send(message);
            }
            MailMessage m = new MailMessage();
        }

        private static void TestPop3()
        {
            Pop3Client c = new Pop3Client("username", "password", "pop3server");
            if (c.Connect())
            {
                Dictionary<int,int> messages =  c.GetMessages();
                foreach (int i in messages.Keys)
                {
                    string message = c.GetMessage(i);
                    c.DeleteMessage(i);
                }
                c.Disconnect();
            }
        }
    }
}
