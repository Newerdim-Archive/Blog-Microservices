using Authentication.API.Extensions;
using FluentValidation;

namespace Authentication.UnitTests.Mocks
{
    public class FluentValidatorsExtensionsTestValidator : AbstractValidator<FluentValidatorsExtensionsTestClass>
    {
        public FluentValidatorsExtensionsTestValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.Username)
                .Username();

            RuleFor(x => x.Password)
                .Password(true);

            RuleFor(x => x.Url)
                .Url();
        }
    }
}