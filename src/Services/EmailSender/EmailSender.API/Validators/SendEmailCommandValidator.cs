using EventBus.Commands;
using FluentValidation;

namespace EmailSender.API.Validators
{
    public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
    {
        public SendEmailCommandValidator()
        {
            RuleFor(x => x.From)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.To)
                .NotEmpty();

            RuleForEach(x => x.To)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
