using AutoFixture;
using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.IntergrationTests.Controllers
{
    class AuthControllerTests
    {

        private static string CreateValidUsername()
        {
            var fixture = new Fixture();

            var usernamePattern = @"^[A-Za-z0-9]{3}(?:[ _-][A-Za-z0-9]+){6}$";
            var username = new SpecimenContext(fixture)
                .Resolve(new RegularExpressionRequest(usernamePattern));

            return username.ToString().Substring(0, 20);
        }

        private static string CreateValidPassword()
        {
            var fixture = new Fixture();

            var passwordPattern = @"^[A-Z]{3}[a-z]{3}[0-9]{3}$";
            var password = new SpecimenContext(fixture)
                .Resolve(new RegularExpressionRequest(passwordPattern));

            return password.ToString();
        }

        private static string CreateValidEmail()
        {
            var fixture = new Fixture();

            return fixture.Create<MailAddress>().Address;
        }
    }
}
