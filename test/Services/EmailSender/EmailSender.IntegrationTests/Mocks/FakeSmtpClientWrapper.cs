using EmailSender.API.Wrappers;
using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;

namespace EmailSender.IntegrationTests.Mock
{
    public class FakeSmtpClientWrapper : ISmtpClientWrapper, IDisposable
    {
        private readonly SmtpClient _client;
        private readonly string _tempFolderPath;

        private static string GetTempFolderPath()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var tempFolder = Path.Combine(Path.GetTempPath(), assemblyName);
            return Path.Combine(tempFolder, "MailMessageTemp");
        }

        public FakeSmtpClientWrapper()
        {
            _tempFolderPath = GetTempFolderPath();

            if (!Directory.Exists(_tempFolderPath))
            {
                Directory.CreateDirectory(_tempFolderPath);
            }

            _client = new SmtpClient("smtpHost")
            {
                UseDefaultCredentials = true,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = _tempFolderPath
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
