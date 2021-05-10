namespace EventBus.Messages
{
    public class NewUserMessage
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}