namespace Authentication.API.Dtos
{
    public record PublishEmailConfirmationRequest
    {
        public string Token { get; init; }
        public string TargetEmail { get; init; }
        public string EmailConfirmationUrl { get; init; }
    }
}