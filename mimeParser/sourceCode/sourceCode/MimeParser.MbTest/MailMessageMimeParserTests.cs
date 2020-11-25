using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Amende.Snorre.Tests
{
    [TestClass]
    public class MailMessageMimeParserTests
    {
        [TestMethod]
        public void TestImportPlainMessage()
        {
            Stream stream = Assembly.GetAssembly(typeof(MailMessageMimeParserTests)).GetManifestResourceStream("MimeParser.MbTest.MailMessages.plaintext.eml");
            StreamReader reader = new StreamReader(stream);
            MailMessage message = MailMessageMimeParser.ParseMessage(new StringReader(reader.ReadToEnd()));
            Assert.AreEqual(message.Subject, "swr123 - Documents Production - Fail");
            Assert.AreEqual(message.Body.Contains("Service Monitor\n===================="), true);
            Assert.AreEqual(message.IsBodyHtml, false);
        }

        [TestMethod]
        public void TestImportMultipartWithAttachment()
        {
            Stream stream = Assembly.GetAssembly(typeof(MailMessageMimeParserTests)).GetManifestResourceStream("MimeParser.MbTest.MailMessages.MultipartWithAttachment.eml");
            StreamReader reader = new StreamReader(stream);
            MailMessage message = MailMessageMimeParser.ParseMessage(new StringReader(reader.ReadToEnd()));
            Assert.AreEqual(message.Subject, "Message with Attachment");
            Assert.AreEqual(string.IsNullOrEmpty( message.Body),true);

            Assert.AreEqual(message.AlternateViews.Count, 2);
            Assert.AreEqual(message.Attachments.Count, 1);
            Assert.AreEqual(message.Attachments[0].Name, "Book1.xlsx");
        }

        [TestMethod]
        public void TestImportMultipartWithEmbeddeImage()
        {
            Stream stream = Assembly.GetAssembly(typeof(MailMessageMimeParserTests)).GetManifestResourceStream("MimeParser.MbTest.MailMessages.MultipartWithEmbeddedImage.eml");
            StreamReader reader = new StreamReader(stream);
            MailMessage message = MailMessageMimeParser.ParseMessage(new StringReader(reader.ReadToEnd()));
            Assert.AreEqual(message.Subject, "Message with embedded image");
            Assert.AreEqual(string.IsNullOrEmpty(message.Body), true);
            Assert.AreEqual(message.AlternateViews.Count, 2);
            Assert.AreEqual(message.AlternateViews[1].LinkedResources.Count, 1);
            Assert.AreEqual(message.Attachments.Count, 0);
        }

    }
}
