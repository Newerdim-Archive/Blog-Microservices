using System;

namespace EmailSender.API.Helper
{
    public class RabbitMqConfiguration
    {
        public string Hostname { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Uri Uri => new($"{Hostname}:{Port}");
    }
}