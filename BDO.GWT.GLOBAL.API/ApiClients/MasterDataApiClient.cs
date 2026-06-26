using AutoMapper;
using BDO.GWT.Common.Extensions;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Web.Core.API.Data;
using BDO.GWT.Web.Core.API.Options;
using Microsoft.Extensions.Options;
using System.Net;

namespace BDO.GWT.Web.Core.API.ApiClients;

public interface IMasterDataApiClient
{
    Task<IEnumerable<MemberFirm>> GetMemberFirmsAsync(CancellationToken cancellationToken = default);

    Task<MemberFirm?> GetMemberFirmAsync(int memberFirmId, CancellationToken cancellationToken = default);
}

public class MasterDataApiClient : IMasterDataApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;
    private readonly IOptions<MasterDataApiOptions> _options;

    private HttpClient? _httpClient;

    public MasterDataApiClient(IHttpClientFactory httpClientFactory, IMapper mapper, IOptions<MasterDataApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _mapper = mapper;
        _options = options;
    }

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
