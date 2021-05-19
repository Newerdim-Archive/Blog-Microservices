using EmailSender.API.Wrappers;
using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;

namespace EmailSender.IntegrationTests.Mock
{
    public class FakeSmtpClientWrapper : ISmtpClientWrapper
    {
        private readonly string _tempFolderPath;

        public FakeSmtpClientWrapper()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var tempFolder = Path.Combine(Path.GetTempPath(), assemblyName);
            _tempFolderPath = Path.Combine(tempFolder, "MailMessageTemp");

            if (!Directory.Exists(_tempFolderPath))
            {
                Directory.CreateDirectory(_tempFolderPath);
            }
        }

        public async Task SendMailAsync(MailMessage message)
        {
            using var client = new SmtpClient("smtpHost")
            {
                UseDefaultCredentials = true,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = _tempFolderPath
            };

            await client.SendMailAsync(message);
        }
    }
}
