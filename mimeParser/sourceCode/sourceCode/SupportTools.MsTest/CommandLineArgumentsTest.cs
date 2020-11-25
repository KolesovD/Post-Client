using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amende.Snorre.SupportTools;

namespace Amende.Snorre.SupportTools.MsTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CommandLineArgumentsTest
    {

        [TestMethod]
        public void TestSingleValue()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentSingleValue arg1 = new CommandLineArgumentSingleValue("pop3server", "enter name of server", true);
            a.Arguments["pop3server"] = arg1;
            bool result = a.Parse(new string[] { "/pop3server:theserver" });
            Assert.AreEqual(arg1.Value, "theserver");
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);

        }
        [TestMethod]
        public void UnknownArgument()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            bool result = a.Parse(new string[] { "/pop3server:theserver" });
            Assert.AreEqual(result, false);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), false);
        }
        [TestMethod]
        public void ArgumentMissing()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentSingleValue arg1 = new CommandLineArgumentSingleValue("pop3server", "enter name of server", true);
            a.Arguments["pop3server"] = arg1;
            bool result = a.Parse(new string[] { });
            Assert.AreEqual(result, false);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), false);
        }


        [TestMethod]
        public void TestExists()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentExists arg1 = new CommandLineArgumentExists("deleteItems", "Deletes messages from pop3 server after retrieval.", true);
            a.Arguments["deleteItems"] = arg1;
            bool result = a.Parse(new string[] { "/deleteItems" });
            Assert.AreEqual(arg1.IsSet, true);
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);
            a.Clear();
            a.Parse(new string[] { });
            Assert.AreEqual(arg1.IsSet, false);
        }
        [TestMethod]
        public void TestBoolean()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentBoolean arg1 = new CommandLineArgumentBoolean("doStuff", "Deletes messages from pop3 server after retrieval.", false);
            a.Arguments["doStuff"] = arg1;
            bool result = a.Parse(new string[] { "/doStuff:true" });
            Assert.AreEqual(arg1.IsSet, true);
            Assert.AreEqual(arg1.Value, true);
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);
            a.Clear();
            Assert.AreEqual(a.Parse(new string[] { "/doStuff:trfgue" }), false);
        }

        [TestMethod]
        public void TestMultiValue()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentMultipleValues arg1 = new CommandLineArgumentMultipleValues("arrayOfNames", "A list of names.", true);
            a.Arguments["arrayOfNames"] = arg1;
            bool result = a.Parse(new string[] { "/arrayOfNames:hei,hopp,\"hei hopp\",\"hei, hopp\"" });
            Assert.AreEqual(arg1.IsSet, true);
            Assert.AreEqual(arg1.Value.Length, 4);
            Assert.AreEqual(arg1.Value[3], "hei, hopp");
            Assert.AreEqual(result, true);
            Assert.AreEqual(string.IsNullOrEmpty(a.ErrorMessage), true);
        }
        
        [TestMethod]
        public void MutuallyExclusiveArguments()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentExists arg1 = new CommandLineArgumentExists("operationA", "does things", false);
            CommandLineArgumentExists arg2 = new CommandLineArgumentExists("operationB", "does other things", false);
            CommandLineArgumentExists arg3 = new CommandLineArgumentExists("operationC", "does even other things", false);
            a.Arguments[arg1.Name] = arg1;
            a.Arguments[arg2.Name] = arg2;
            a.Arguments[arg3.Name] = arg3;
            a.AddMutuallyExclusive(new List<CommandLineArgument>() { arg1, arg2, arg3 },true);
            bool result = a.Parse(new string[] { "/operationA" });
            Assert.AreEqual(result, true);
            a.Clear();
            result = a.Parse(new string[] { "/operationA", "/operationB" });
            Assert.AreEqual(result, false);

            a.Clear();
            result = a.Parse(new string[] { "/operationB", "/operationC" });
            Assert.AreEqual(result, false);

            a.Clear();
            result = a.Parse(new string[] { "/operationC", "/operationA" });
            Assert.AreEqual(result, false);

            a.Clear();
            result = a.Parse(new string[] { "/operationC", "/operationA", "/operationB" });
            Assert.AreEqual(result, false);

            a.Clear();
            result = a.Parse(new string[] { "" });
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void Dependencies()
        {
            CommandLineArguments a = new CommandLineArguments("Testtool v0.2\nUsage: the tool is not used");
            CommandLineArgumentExists arg1 = new CommandLineArgumentExists("operationA", "does things", false);
            CommandLineArgumentSingleValue arg2 = new CommandLineArgumentSingleValue("specify", "a value",true);
            CommandLineArgumentSingleValue arg3 = new CommandLineArgumentSingleValue("specify2", "a value", false);
            a.Arguments[arg1.Name] = arg1;
            a.Arguments[arg2.Name] = arg2;
            a.Arguments[arg3.Name] = arg3;
            arg2.DependsOn = arg1;
            arg3.DependsOn = arg1;

            bool result = a.Parse(new string[] { "/operationA", "/specify:abc", "/specify2:abc" });
            Assert.AreEqual(result, true);
            a.Clear();
            result = a.Parse(new string[] { "/operationA", "/specify:abc" });
            Assert.AreEqual(result, true);
            a.Clear(); 
            result = a.Parse(new string[] { "/operationA", "/specify2:abc" });
            Assert.AreEqual(result, false);
            a.Clear();

            result = a.Parse(new string[] { "/operationA"});
            Assert.AreEqual(result, false);
            a.Clear();
            result = a.Parse(new string[] { "/specify:abc" });
            Assert.AreEqual(result, false);
        }
    }
}
