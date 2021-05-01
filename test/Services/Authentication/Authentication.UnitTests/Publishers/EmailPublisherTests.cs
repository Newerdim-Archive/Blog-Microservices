using Authentication.API.Helpers;
using Authentication.API.Publishers;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.UnitTests.Publishers
{
    class EmailPublisherTests
    {
        private readonly Mock<IOptions<CompanySettings>> _companySettingsOptionsMock = new();
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

        public EmailPublisherTests()
        {
            var companySettings = new CompanySettings
            {
                Email = "test@company.com",
                Name = "company"
            };

            _companySettingsOptionsMock.Setup(x => x.Value).Returns(companySettings);
        }

        private IEmailPublisher CreatePublisher()
        {
            return new EmailPublisher(
                _publishEndpointMock.Object, 
                _companySettingsOptionsMock.Object);
        }


    }
}
