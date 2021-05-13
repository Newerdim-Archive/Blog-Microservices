using System;

namespace EmailSender.API.Helper
{
    public class RabbitMqSettings
    {
        public string Hostname { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Contains hostname and port
        /// </summary>
        public Uri Uri => new($"{Hostname}:{Port}");
    }
}