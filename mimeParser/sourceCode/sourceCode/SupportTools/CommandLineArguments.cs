using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Amende.Snorre.SupportTools
{
    public enum CommandLineArgumentType
    {
        Boolean,
        Exists,
        SingleValue,
        MultipleValues,
        Integer
    }

    public abstract class CommandLineArgument
    {
        public Dictionary<string, CommandLineArgument> Arguments { get; set; }

        public List<CommandLineArgument> Requires { get; set; }
        public CommandLineArgument DependsOn { get; set; }
        public string Usage { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
        public bool IsSet { get; set; }
        public abstract bool SetValue(string value);
        public CommandLineArgument(string name, string usage, bool required)
        {
            Name = name;
            Usage = usage;
            Required = required;
            IsSet = false;
        }
    }
    public class CommandLineArgumentSingleValue : CommandLineArgument
    {
        public string Value { get; set; }
        public CommandLineArgumentSingleValue(string name, string usage, bool required):
            base(name,usage,required)
        {
        }
        public CommandLineArgumentSingleValue(string name, string usage, bool required, string defaultValue) :
            base(name, usage, required)
        {
            Value = defaultValue;
            IsSet = true;
        }
        public override bool SetValue(string value)
        {
            Value = value;
            return true;
        }
    }
    public class CommandLineArgumentMultipleValues : CommandLineArgument
    {
        public string[] Value { get; set; }
        public CommandLineArgumentMultipleValues(string name, string usage, bool required) :
            base(name, usage, required)
        {
        }
        public CommandLineArgumentMultipleValues(string name, string usage, bool required, string defaultValue) :
            base(name, usage, required)
        {
            Value = StringTools.SplitQuoted(defaultValue);
            IsSet = true;
        }
        public override bool SetValue(string value)
        {
            //Value = new List<string>(Regex.Split(value, ","));
            Value = StringTools.SplitQuoted(value);
            return true;
        }
    }

    public class CommandLineArgumentBoolean : CommandLineArgument
    {
        public bool Value { get; set; }
        public CommandLineArgumentBoolean(string name, string usage, bool required) :
            base(name, usage, required)
        {
        }
        public CommandLineArgumentBoolean(string name, string usage, bool required, bool defaultValue) :
            base(name, usage, required)
        {
            Value = defaultValue;
            IsSet = true;
        }
        public override bool SetValue(string value)
        {
            bool result = false;
            bool tmpValue = Boolean.TryParse(value,out result);
            if (result)
            {
                Value = tmpValue;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
    public class CommandLineArgumentExists : CommandLineArgument
    {
        public CommandLineArgumentExists(string name, string usage, bool required):
            base(name, usage, required)
        {
        }
        public override bool SetValue(string value)
        {
            return true;
        }
    }

    public class CommandLineArguments
    {
        private class MutuallyExclusiveCollection
        {
            public List<CommandLineArgument> Arguments { get; set; }
            public bool OneIsRequired { get; set; }
        }

        private List<MutuallyExclusiveCollection> MutuallyExclusive { get; set; }
        
        public Dictionary<string,CommandLineArgument> Arguments { get; set; }
        public string HelpText { get; set; }
        public string ErrorMessage { get; set; }
        public CommandLineArguments(string helpText)
        {
            Arguments = new Dictionary<string,CommandLineArgument>();
            MutuallyExclusive = new List<MutuallyExclusiveCollection>();
            HelpText = helpText;
        }

        public void Clear()
        {
            foreach (CommandLineArgument arg in Arguments.Values)
            {
                arg.IsSet = false;
            }
        }
        public bool Parse(string[] args)
        {
            bool returnValue = true ;
            StringBuilder errorMessage = new StringBuilder();
            foreach (string s in args)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                int pos = s.IndexOf(':');
                string key = string.Empty;
                string value = string.Empty;
                if (pos > 1)
                {
                    key = s.Substring(1, pos-1);
                    value = s.Substring(pos+1);
                }
                else
                    key = s.Substring(1);

                if (!Arguments.ContainsKey(key))
                {
                    returnValue = false;
                    errorMessage.AppendLine("Unknown argument \"" + key + "\"");
                }
                else
                {
                    CommandLineArgument arg = Arguments[key];
                    arg.IsSet = true;
                    if (!arg.SetValue(value))
                    {
                        errorMessage.AppendLine("Illegal argument \"" + value + "\" for argument \"" + key + "\"");
                        returnValue = false;
                    }
                }
            }
            foreach (CommandLineArgument arg in Arguments.Values)
            {
                if(arg.Required && !arg.IsSet)
                {
                    errorMessage.AppendLine("Missing argument:\"" + arg.Name + "\"");
                    returnValue = false;
                }
            }
            CommandLineArgument dummy = null;
            if(returnValue)
                returnValue = check(ref dummy);
            if (returnValue)
                returnValue = CheckMutuallyExclusive(errorMessage);
            if(!returnValue)
                ErrorMessage = errorMessage.ToString();
            return returnValue;
        }
        private bool check(ref CommandLineArgument dep)
        {
            bool returnValue = true;
            CommandLineArgument[] args = Arguments.Values.ToArray<CommandLineArgument>();

            for (int i = 0; i < args.Length; i++)
            {
                if(args[i].DependsOn == dep)
                {
                    if (args[i].Required && !args[i].IsSet)
                    {
                        returnValue = false;
                        break;
                        //errormessage+="urgl";
                    }
                    else if (dep != null && args[i].IsSet && !dep.IsSet)
                    {
                        returnValue = false;
                        break;
                    }
                     //else if(!args[i].IsSet)
                     returnValue = check(ref args[i]);
                }
            }
            return returnValue;
        }

        private bool CheckRequired(CommandLineArgument dependantArg, StringBuilder errorMessage)
        {
            bool returnValue = true;
            foreach (CommandLineArgument arg in Arguments.Values)
            {
                if (arg.DependsOn == dependantArg)
                {
                    if (arg.Required && !arg.IsSet)
                    {
                        errorMessage.AppendLine("Missing argument:\"" + arg.Name + "\"");
                        returnValue = false;
                    }
                }
            }
            return returnValue;
        }
        private bool CheckMutuallyExclusive(StringBuilder errorMessage)
        {
            bool returnValue = true;
            foreach (MutuallyExclusiveCollection argList in MutuallyExclusive)
            {
                bool foundOne = false;

                foreach (CommandLineArgument mutexArg in argList.Arguments)
                {

                    foreach (CommandLineArgument arg in Arguments.Values)
                    {

                        if (arg == mutexArg && arg.IsSet)
                        {
                            if (!foundOne)
                            {
                                foundOne = true;
                                break;
                            }
                            else
                            {
                                returnValue = false;
                                break;
                            }
                        }
                    }
                    if (!returnValue)
                        break;
                    
                }
                if (returnValue && argList.OneIsRequired && !foundOne)
                {
                    returnValue = false;
                }
                if (!returnValue)
                {
                    StringBuilder errorMessageArgList = new StringBuilder();
                    foreach (CommandLineArgument mutexArg in argList.Arguments)
                    {
                        errorMessageArgList.Append(" /" +mutexArg.Name);
                    }
                    if (argList.OneIsRequired && !foundOne)
                        errorMessage.AppendLine("One of" + errorMessageArgList.ToString() + " must be specified.");
                    else
                        errorMessage.AppendLine("Only one of" + errorMessageArgList.ToString() + " can be specified.");
                }
            }
            return returnValue;
        }


        public void AddMutuallyExclusive(List<CommandLineArgument> list, bool isOneRequired)
        {
            MutuallyExclusive.Add(new MutuallyExclusiveCollection { Arguments = list, OneIsRequired = isOneRequired });
        }
    }
}
