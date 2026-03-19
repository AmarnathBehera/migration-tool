using BDO.GWT.Core.DTOs.Header;
using System.Threading;
using System.Threading.Tasks;
using BDO;
using Kentico.Xperience.Admin.Base;
using BDO.GWT.Core.DTOs;
namespace BDO.GWT.Core.Services;

public interface IFooterService
{
    Task<FooterDto> GetAsync(Page currentPage, CancellationToken cancellationToken);
}

public class FooterService : IFooterService
{
    public FooterService()
    {
    }

    public Task<FooterDto> GetAsync(Page currentPage, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
