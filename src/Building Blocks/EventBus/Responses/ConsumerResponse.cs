namespace EventBus.Results
{
    public class ConsumerResponse
    {
        public bool Successful { get; set; }

        public ConsumerResponse(bool successful = true)
        {
            Successful = successful;
        }
    }
}
