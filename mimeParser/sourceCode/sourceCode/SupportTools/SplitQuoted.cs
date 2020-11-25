using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Amende.Snorre.SupportTools
{
    public static class StringTools
    {
        public static string[] SplitQuoted(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text", "text is null.");
            ArrayList res = new ArrayList();
            string pattern = @"""([^""\\]*[\\.[^""\\]*]*)""|([^,]+)";

            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, pattern))
            {
                string wQuotes = m.Groups[1].Value;
                string woQuotes = m.Groups[2].Value;
                if (string.IsNullOrEmpty(wQuotes))
                {
                    res.Add(woQuotes);
                }
                else
                {
                    res.Add(wQuotes);
                }
            }
            
            return (string[])res.ToArray(typeof(string));
        }
    }
}
