using BDO.GWT.Domain.Services;
using BDO.GWT.Xperience;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Common.LiveSite.Services;

public interface IScriptsService
{
    Task<string> GetHeadScriptsAsync(CancellationToken cancellationToken);

    Task<string> GetTopBodyScriptsAsync(CancellationToken cancellationToken);

    Task<string> GetBottomBodyScriptsAsync(CancellationToken cancellationToken);
}

public class ScriptsService : IScriptsService
{
    private readonly ISettingsService _settingsService;

    public ScriptsService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        
    }

    public async Task<string> GetHeadScriptsAsync(CancellationToken cancellationToken)
    {

        var settings = await _settingsService.GetCurrentWebsiteSettingsAsync(new[]
        {
            "BDO_Global_HeadCode",
          

        }, cancellationToken);

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append((string?)settings["BDO_Global_HeadCode"]);
       
        return scriptBuilder.ToString();
    }

    public async Task<string> GetTopBodyScriptsAsync(CancellationToken cancellationToken)
    {
        var setting = await _settingsService.GetCurrentWebsiteSettingAsync("BDO_Global_HeadCode", cancellationToken);

        return (string?)setting?.Value!;
    }

    public async Task<string> GetBottomBodyScriptsAsync(CancellationToken cancellationToken)
    {
        var setting = await _settingsService.GetCurrentWebsiteSettingAsync("BDO_Global_HeadCode", cancellationToken);

        return (string?)setting?.Value!;
    }
}
