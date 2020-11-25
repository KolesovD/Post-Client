using System;
using System.Collections.Generic;

namespace JohnGietzen.MailUtilities
{
    public class AccountDescriptor
    {
        public String Username { get { return _Username; } }
        private String _Username;

        public String WorkingDir { get { return _WorkingDir; } }
        private String _WorkingDir;

        public List<CacheDescriptor> Caches = new List<CacheDescriptor>();

        public AccountDescriptor(String username, String workingDir)
        {
            _Username = username;
            _WorkingDir = workingDir;
        }
    }
}
