namespace EventBus.Events
{
    public class SendEmailEvent
    {
        public string From { get; set; }

        public string[] To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}

