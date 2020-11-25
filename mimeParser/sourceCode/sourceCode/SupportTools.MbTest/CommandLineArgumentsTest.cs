using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Amende.Snorre.SupportTools;

namespace Amende.Snorre.Tests
{
    [TestFixture]
    public class CommandLineArgumentsTest
    {
        [Test]
        public void SimpleTest()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentSingleValue arg1 = new CommandLineArgumentSingleValue("pop3server","enter name of server",true);
            a.Arguments["pop3server"] = arg1;
            bool result = a.Parse(new string[]{"/pop3server:theserver"});
            Assert.AreEqual(arg1.Value, "theserver");
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);
        }

        [Test]
        public void ArgumentMissing()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentSingleValue arg1 = new CommandLineArgumentSingleValue("pop3server", "enter name of server", true);
            a.Arguments["pop3server"] = arg1;
            bool result = a.Parse(new string[] { });
            Assert.AreEqual(result, false);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), false);
        }


        [Test]
        public void TestExists()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentExists arg1 = new CommandLineArgumentExists("deleteItems", "Deletes messages from pop3 server after retrieval.", true);
            a.Arguments["deleteItems"] = arg1;
            bool result = a.Parse(new string[] {"/deleteItems"});
            Assert.AreEqual(arg1.IsSet, true);
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);
            a.Clear();
            a.Parse(new string[] { });
            Assert.AreEqual(arg1.IsSet, false);
        }

        [Test]
        public void TestMultiValue()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentMultipleValues arg1 = new CommandLineArgumentMultipleValues("arrayOfNames", "A list of names.", true);
            a.Arguments["arrayOfNames"] = arg1;
            bool result = a.Parse(new string[] { "/arrayOfNames:1,2,3,4" });
            Assert.AreEqual(arg1.IsSet, true);
            Assert.AreEqual(arg1.Value.Count, 4);
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);
        }
    }
}
