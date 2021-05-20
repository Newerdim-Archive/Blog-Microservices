namespace EventBus.Results
{
    public class ConsumerBaseResult
    {
        public bool Successful { get; set; }

        public ConsumerBaseResult(bool successful = true)
        {
            Successful = successful;
        }
    }
}
