using BDO.GWT.Domain.Core.Services;
using Microsoft.Extensions.Configuration;

namespace BDO.GWT.Xperience.Core.Services;
public interface IHashSaltProvider
{
    public string? HashSalt { get; }
}
public class HashSaltProvider : IHashSaltProvider
{
    private const string HashSaltSettingKey = "CMSHashStringSalt";

    private readonly IConfiguration _configuration;

    public HashSaltProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? HashSalt => _configuration[HashSaltSettingKey];
}
