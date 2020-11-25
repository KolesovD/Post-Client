using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace JohnGietzen.MailUtilities
{
    class SpamhausFilter : SpamFlter
    {
        public override SpamFilterResult FilterSpam(MimeMessage message)
        {
            SpamFilterResult Result = new SpamFilterResult();

            // Match the ips in the message
            MatchCollection Matches = Regex.Matches(message.RootNode.RawHeader, @"from ([-A-Za-z0-9.]+|\[\d+\.\d+\.\d+\.\d+\]) \(([-A-Za-z0-9.]+ )?\[(?<ipaddress>(?<a>\d+)\.(?<b>\d+)\.(?<c>\d+)\.(?<d>\d+)(?<port>:\d+)?)\]\)", RegexOptions.Multiline);

            List<String> Lookups = new List<String>();
            foreach (Match match in Matches)
            {
                int IPClass = int.Parse(match.Groups["a"].Value);

                // Reserved addresses (eg, loopback address) should be discarded.
                if (IPClass == 0 || IPClass == 127)
                    continue;
                // Local networks should be discarded, but these may be overly generic...
                if (IPClass == 10)
                    continue;
                if (IPClass == 192)
                    continue;

                String Lookup = match.Groups["d"].Value + "." +
                                match.Groups["c"].Value + "." +
                                match.Groups["b"].Value + "." +
                                match.Groups["a"].Value + "." +
                                "zen.spamhaus.org";
                if (Lookups.Contains(Lookup))
                    continue;

                Lookups.Add(Lookup);
            }
            Lookups.Reverse();

            foreach (String Lookup in Lookups)
            {
                int SpamhausLevel = 0;
                try
                {
                    IPHostEntry iphe = Dns.GetHostEntry(Lookup);
                    byte[] address = iphe.AddressList[0].GetAddressBytes();
                    SpamhausLevel = (int)address[3];
                }
                catch (SocketException)
                {
                    SpamhausLevel = 1;
                }

                
                if (SpamhausLevel == 2)
                {
                    Result.IsSpam = true;
                    Result.Reason = "Spamhaus SBL (main block list)";
                    return Result;
                }
                else if(SpamhausLevel >= 4 && SpamhausLevel <= 8)
                {
                    Result.IsSpam = true;
                    Result.Reason = "Spamhaus XBL (exploits block list)";
                    return Result;
                }
                else if (SpamhausLevel >= 10 && SpamhausLevel <= 11)
                {
                    Result.IsSpam = true;
                    Result.Reason = "Spamhaus PBL (policy block list)";
                    return Result;
                }
            }

            Result.IsSpam = false;
            return Result;
        }

        public override void FeedbackResult(MimeMessage message, SpamFilterResult result)
        {
            // Spamhaus filter does not need feedback.
            return;
        }

        public override void Save()
        {
            // Spamhaus filter does not need to save.
            return;
        }
    }
}
