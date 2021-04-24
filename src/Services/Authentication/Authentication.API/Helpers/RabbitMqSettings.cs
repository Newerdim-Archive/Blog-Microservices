using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailSender.API.Helper
{
    public class RabbitMqSettings
    {
        public string Hostname { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Uri Uri => new ($"{Hostname}:{Port}");
    }
}
