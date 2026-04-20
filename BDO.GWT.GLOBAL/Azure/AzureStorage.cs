namespace GWTXbyKWeb.Azure
{
    public class AzureStorage
    {
        public string RootContainer { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string SharedKey { get; init; } = string.Empty;
        public string? CdnEndpoint { get; init; }
        public bool IsPublicContainer { get; init; }
    }
}
