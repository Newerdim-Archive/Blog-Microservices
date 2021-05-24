using EmailSender.API.Dtos;
using FluentValidation;

namespace EmailSender.API.Validators
{
    public class SendRequestValidator : AbstractValidator<SendRequest>
    {
        public SendRequestValidator()
        {
            RuleFor(x => x.To)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.From)
                .NotEmpty()
                .EmailAddress();
        }
    }
}