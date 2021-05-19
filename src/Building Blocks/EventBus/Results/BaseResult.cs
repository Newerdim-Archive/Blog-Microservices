namespace EventBus.Results
{
    public class BaseResult
    {
        public bool Successful { get; set; }

        /// <summary>
        /// Create successful result
        /// </summary>
        public BaseResult()
        {
            Successful = true;
        }

        public BaseResult(bool successful)
        {
            Successful = successful;
        }
    }
}
