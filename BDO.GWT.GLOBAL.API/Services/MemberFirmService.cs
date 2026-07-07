using AutoMapper;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.ApiClients;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Service interface for managing member firms, providing methods to retrieve member firm information.
/// </summary>
public interface IMemberFirmService
{
    /// <summary>
    /// Retrieves a member firm by its ID, utilizing caching for improved performance.
    /// </summary>
    /// <param name="memberFirmId">The ID of the member firm to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The member firm if found; otherwise, null.</returns>
    Task<MemberFirm?> GetMemberFirmAsync(int memberFirmId, CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of the IMemberFirmService interface, responsible for managing member firms and providing methods to retrieve member firm information.
/// </summary>
public class MemberFirmService : IMemberFirmService
{
    private const string CacheKey = "MemberFirmList";

    private readonly IMapper _mapper;
    private readonly IMasterDataApiClient _masterDataApiClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<MemberFirmServiceOptions> _options;

    /// <summary>
    /// Initializes a new instance of the MemberFirmService class with the specified dependencies.
    /// </summary>
    /// <param name="mapper"></param>
    /// <param name="masterDataApiClient"></param>
    /// <param name="memoryCache"></param>
    /// <param name="options"></param>
    public MemberFirmService(IMapper mapper, IMasterDataApiClient masterDataApiClient, IMemoryCache memoryCache, IOptions<MemberFirmServiceOptions> options)
    {
        _mapper = mapper;
        _masterDataApiClient = masterDataApiClient;
        _memoryCache = memoryCache;
        _options = options;
    }

    /// <summary>
    /// Retrieves a member firm by its ID, utilizing caching for improved performance. If the member firm is not found in the cache, it will be fetched from the master data API and added to the cache.
    /// </summary>
    /// <param name="memberFirmId">The ID of the member firm to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The member firm if found; otherwise, null.</returns>
    public async Task<MemberFirm?> GetMemberFirmAsync(int memberFirmId, CancellationToken cancellationToken)
    {
        var memberFirmById = _memoryCache.Get<IDictionary<int, MemberFirm>>(CacheKey);

        if (memberFirmById == null)
        {
            memberFirmById = await BuildMemberFirmDictionaryAsync(cancellationToken);

            _memoryCache.Set(CacheKey, memberFirmById, _options.Value.CachingPeriod);
        }

        if (memberFirmById.TryGetValue(memberFirmId, out var memberFirm))
        {
            return memberFirm;
        }

        memberFirm = await _masterDataApiClient.GetMemberFirmAsync(memberFirmId, cancellationToken);

        return memberFirm;
    }

    private async Task<IDictionary<int, MemberFirm>> BuildMemberFirmDictionaryAsync(CancellationToken cancellationToken)
    {
        var memberFirmById = new Dictionary<int, MemberFirm>();

        if (_options.Value.IncludeMemberFirmsJson)
        {
            var memberFirmsJsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "memberfirms.json");

            if (File.Exists(memberFirmsJsonFilePath))
            {
                await using var fileStream = File.OpenRead(memberFirmsJsonFilePath);

                var nonProductionMemberFirmDataEntries = await JsonSerializer.DeserializeAsync<List<MemberFirmData>>(fileStream, cancellationToken: cancellationToken) ?? throw new System.Runtime.Serialization.SerializationException("Failed to deserialize 'memberfirms.json' configuration file.");

                var nonProductionMemberFirms = _mapper.Map<IEnumerable<MemberFirm>>(nonProductionMemberFirmDataEntries);

                foreach (var memberFirm in nonProductionMemberFirms)
                {
                    memberFirmById.Add(memberFirm.ID, memberFirm);
                }
            }
        }

        var memberFirmsFromMasterData = await _masterDataApiClient.GetMemberFirmsAsync(cancellationToken);

        foreach (var memberFirm in memberFirmsFromMasterData)
        {
            memberFirmById.TryAdd(memberFirm.ID, memberFirm);
        }

        return memberFirmById;
    }
}
