using BDO.GWT.Core.DTOs.Header;
using System.Threading;
using System.Threading.Tasks;
using BDO;
using Kentico.Xperience.Admin.Base;
namespace BDO.GWT.Core.Services;

public interface IHeaderService
{
    Task<HeaderDto> GetAsync(Page currentPage, CancellationToken cancellationToken);
}

public class HeaderService : IHeaderService
{
    public HeaderService()
    {
    }

    public Task<HeaderDto> GetAsync(Page currentPage, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
