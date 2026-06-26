using AutoMapper;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Web.Core.API.ApiClients;
using BDO.GWT.Web.Core.API.Data;
using BDO.GWT.Web.Core.API.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.API.Services;

public interface IMemberFirmService
{
    Task<MemberFirm?> GetMemberFirmAsync(int memberFirmId, CancellationToken cancellationToken);
}

public class MemberFirmService : IMemberFirmService
{
    private const string CacheKey = "MemberFirmList";

    private readonly IMapper _mapper;
    private readonly IMasterDataApiClient _masterDataApiClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<MemberFirmServiceOptions> _options;

    public MemberFirmService(IMapper mapper, IMasterDataApiClient masterDataApiClient, IMemoryCache memoryCache, IOptions<MemberFirmServiceOptions> options)
    {
        _mapper = mapper;
        _masterDataApiClient = masterDataApiClient;
        _memoryCache = memoryCache;
        _options = options;
    }

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
