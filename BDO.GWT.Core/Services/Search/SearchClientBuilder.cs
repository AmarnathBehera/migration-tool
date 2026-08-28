using Azure;
using Azure.Search.Documents;
using BDO.GWT.Core.Extensions;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace BDO.GWT.AzureCognitiveSearch.Services
{
    public interface ISearchClientBuilder
    {
        public SearchClient CreateSearchClient(string channelName);
    }

    public class SearchClientBuilder : ISearchClientBuilder
    {
        private readonly AzureCognitiveSearchOptions _azureSearchOptions;

        public SearchClientBuilder(IOptions<AzureCognitiveSearchOptions> azureSearchOptions)
        {
            _azureSearchOptions = azureSearchOptions.Value;
        }

        public SearchClient CreateSearchClient(string channelName)
        {
            var preparedChannelName = RemoveSpecialCharacters(channelName).ToLowerInvariant();
            var indexName = $"bdo-{preparedChannelName}{_azureSearchOptions.IndexSuffix}";
            var credentials = new AzureKeyCredential(_azureSearchOptions.QueryKey);

            return new SearchClient(_azureSearchOptions.ServiceUrl, indexName.ToLowerInvariant(), credentials);
        }

        private static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9-]+", "", RegexOptions.Compiled);
        }
    }
}
