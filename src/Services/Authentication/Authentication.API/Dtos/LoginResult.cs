using Authentication.API.Enums;

namespace Authentication.API.Dtos
{
    public class LoginResult
    {
        public int UserId { get; set; }
        public LoginResultMessage Message { get; set; }
    }
}