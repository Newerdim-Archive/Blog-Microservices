using Authentication.API.Extensions;
using FluentValidation;

namespace Authentication.UnitTests.Mocks
{
    public class CustomFluentValidatorsTestValidator : AbstractValidator<CustomFluentValidatorsTestClass>
    {
        public CustomFluentValidatorsTestValidator()
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
