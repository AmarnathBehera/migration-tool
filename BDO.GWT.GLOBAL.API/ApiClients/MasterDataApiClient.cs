using AutoMapper;
using BDO.GWT.Common.Extensions;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Options;
using Microsoft.Extensions.Options;
using System.Net;

namespace BDO.GWT.GLOBAL.API.ApiClients;

/// <summary>
/// Defines the interface for the Master Data API client, which provides methods to retrieve member firm data from the Master Data API.
/// </summary>
public interface IMasterDataApiClient
{
    /// <summary>
    /// Retrieves a list of member firms from the Master Data API. The method filters the results to include only firms of type 'TestFirm' or 'Firm'.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<MemberFirm>> GetMemberFirmsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific member firm by its ID from the Master Data API. If the member firm is not found, the method returns null.
    /// </summary>
    /// <param name="memberFirmId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MemberFirm?> GetMemberFirmAsync(int memberFirmId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of the IMasterDataApiClient interface for retrieving member firm data from the Master Data API.
/// </summary>
public class MasterDataApiClient : IMasterDataApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;
    private readonly IOptions<MasterDataApiOptions> _options;

    private HttpClient? _httpClient;

    /// <summary>
    /// Initializes a new instance of the MasterDataApiClient class with the specified dependencies.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="mapper">The AutoMapper instance for mapping data objects.</param>
    /// <param name="options">The Master Data API configuration options.</param>
    public MasterDataApiClient(IHttpClientFactory httpClientFactory, IMapper mapper, IOptions<MasterDataApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _mapper = mapper;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MemberFirm>> GetMemberFirmsAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateHttpClient();

        using var response = await client.GetAsync("memberFirms?$filter=Type eq 'TestFirm' or Type eq 'Firm'", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve member firms from master data API.");
        }

        var memberFirmDataEntries = await response.Content.ReadFromJsonAsync<IEnumerable<MemberFirmData>>(cancellationToken: cancellationToken);

        if (memberFirmDataEntries == null)
        {
            throw new NullReferenceException("Member firms data after serialization is null.");
        }

        var memberFirms = _mapper.Map<IEnumerable<MemberFirm>>(memberFirmDataEntries);

        return memberFirms;
    }

    /// <inheritdoc />
    public async Task<MemberFirm?> GetMemberFirmAsync(int memberFirmId, CancellationToken cancellationToken = default)
    {
        var client = CreateHttpClient();

        using var response = await client.GetAsync($"memberFirms/{memberFirmId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve fallback member firm from master data.");
        }

        var memberFirmData = await response.Content.ReadFromJsonAsync<MemberFirmData>(cancellationToken: cancellationToken);

        var memberFirm = _mapper.Map<MemberFirm>(memberFirmData!);

        return memberFirm;
    }

    /// <summary>
    /// Creates and configures an HTTP client for communicating with the Master Data API.
    /// </summary>
    /// <returns>A configured HTTP client instance.</returns>
    private HttpClient CreateHttpClient()
    {
        if (_httpClient != null)
        {
            return _httpClient;
        }

        _httpClient = _httpClientFactory.CreateClient("MasterDataApiClient");

        _httpClient.BaseAddress = new Uri(_options.Value.ApiUrl.EnsureEndsWith("/"));
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _options.Value.ApiSubscriptionKey);

        return _httpClient;
    }
}
