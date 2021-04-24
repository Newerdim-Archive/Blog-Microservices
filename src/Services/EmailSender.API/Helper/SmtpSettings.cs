using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailSender.API.Helper
{
    public class SmtpSettings
    {
        public string Hostname { get; set; }

        public int Port { get; set; }
    }
}
