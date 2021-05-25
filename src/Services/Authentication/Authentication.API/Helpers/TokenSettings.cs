namespace Authentication.API.Helpers
{
    public class TokenSettings
    {
        public string EmailConfirmationSecret { get; set; }
        public string AccessTokenSecret { get; set; }
        public string RefreshTokenSecret { get; set; }
    }
}