using Authentication.API.Enums;

namespace Authentication.API.Dtos
{
    public class RegisterResult
    {
        public RegisterResultMessage Message { get; set; }
        public int UserId { get; set; }
    }
}