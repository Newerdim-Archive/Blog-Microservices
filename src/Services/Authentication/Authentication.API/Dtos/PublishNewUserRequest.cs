namespace Authentication.API.Dtos
{
    public class PublishNewUserRequest
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
    }
}