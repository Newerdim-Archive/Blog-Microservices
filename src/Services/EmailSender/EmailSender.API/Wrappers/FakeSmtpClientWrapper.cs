using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;

namespace EmailSender.API.Wrappers
{
    public class FakeSmtpClientWrapper : ISmtpClientWrapper, IDisposable
    {
        private readonly SmtpClient _client;

        public FakeSmtpClientWrapper()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var tempFolder = Path.Combine(Path.GetTempPath(), assemblyName);
            tempFolder = Path.Combine(tempFolder, "MailMessageTemp");

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            _client = new SmtpClient("smtpHost")
            {
                UseDefaultCredentials = true,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = tempFolder
            };
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task SendMailAsync(MailMessage message)
        {
            await _client.SendMailAsync(message);
        }
    }
}
