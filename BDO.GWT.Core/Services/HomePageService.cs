using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface IHomePageService
{
    Task<HomeDto> GetHomePageDtoAsync(HomePage page, CancellationToken cancellationToken = default);
    Task<string?> GetHomePageUrl(CancellationToken cancellationToken);
}

//#Need to implement IHomePageService
public class HomePageService 
{

    private readonly IContentService _ContentRetriever;
    public HomePageService(IContentService PageRetriever)
    {
        _ContentRetriever = PageRetriever;

    }


}

