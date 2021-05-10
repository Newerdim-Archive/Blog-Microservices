using Authentication.API.Dtos;
using Authentication.API.Helpers;
using Authentication.API.Validators;
using EventBus.Events;
using MassTransit;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Authentication.API.Publishers
{
    public class EmailPublisher : IEmailPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly CompanySettings _companySettings;

        public EmailPublisher(IPublishEndpoint publishEndpoint, IOptions<CompanySettings> companySettingsOptions)
        {
            _publishEndpoint = publishEndpoint;
            _companySettings = companySettingsOptions.Value;
        }

        public async Task PublishEmailConfirmationAsync(PublishEmailConfirmationRequest request)
        {
            var validator = new PublishEmailConfirmationRequestValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                throw new ArgumentException($"{typeof(PublishEmailConfirmationRequest)} is invalid. Errors: {string.Concat(result.Errors)}");
            }

            var message = $@"Hi {request.TargetEmail}, <br><br>
                We just need to verify your email address before you can access {_companySettings.Name} account <br><br>
                Verify your email address - <a href=""{request.ReturnUrl}/{request.Token}"">Click here!</a> <br><br>
                Thanks! – The {_companySettings.Name} team";

            var sendEmailEvent = new SendEmailEvent
            {
                To = new string[] { request.TargetEmail },
                Subject = $"{_companySettings.Name} email verification",
                Body = message,
                From = _companySettings.Email
            };

            await _publishEndpoint.Publish(sendEmailEvent);
        }
    }
}
