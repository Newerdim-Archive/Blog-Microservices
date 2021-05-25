namespace Authentication.API.Responses
{
    public class LoginResponse : BaseResponse
    {
        public string AccessToken { get; set; }

        public int UserId { get; set; }
    }
}