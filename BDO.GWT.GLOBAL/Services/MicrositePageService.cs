using BDO.GWT.Core.DTOs.Pages;
using BDO;
using BDO.GWT.Core;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Services;

public interface IMicrositePageService
{
    Task<MicrositePageDto> GetMicrositePageDtoAsync(MicrositePage currentPage, CancellationToken cancellationToken);
}

public class MicrositePageService : IMicrositePageService
{
    private readonly IContentService _contentService;

    public MicrositePageService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<MicrositePageDto> GetMicrositePageDtoAsync(MicrositePage currentPage, CancellationToken cancellationToken)
    {
        var masterPage = await _contentService.RetrieveCurrentPageAsync<MicrositeMasterPage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        var headHtml = CombineHtmlFields(masterPage?.HeadHTML, currentPage.PageHeadHTML);
        var bodyTopHtml = CombineHtmlFields(masterPage?.BodyTopHTML, currentPage.PageTopBodyHTML);
        var bodyBottomHtml = CombineHtmlFields(masterPage?.BodyBottomHTML, currentPage.PageBottomBodyHTML);

        var customCssLinks = masterPage?.CustomCSS?
            .Where(a => a.CssAttachmentAsset1?.Url is not null)
            .Select(a => a.CssAttachmentAsset1.Url!)
            ?? Enumerable.Empty<string>();

        var customJsLinks = masterPage?.CustomJS?
            .Where(a => a.JavaScriptAttachmentAsset1?.Url is not null)
            .Select(a => a.JavaScriptAttachmentAsset1.Url!)
            ?? Enumerable.Empty<string>();

        return new MicrositePageDto
        {
            HeadHtml = headHtml,
            BodyTopHtml = bodyTopHtml,
            BodyBottomHtml = bodyBottomHtml,
            CustomCssLinks = customCssLinks,
            CustomJsLinks = customJsLinks,
        };
    }

    private static string CombineHtmlFields(string htmlField1, string htmlField2)
    {
        if (string.IsNullOrEmpty(htmlField1) && string.IsNullOrEmpty(htmlField2))
        {
            return null;
        }

        if (string.IsNullOrEmpty(htmlField1))
        {
            return htmlField2;
        }

        if (string.IsNullOrEmpty(htmlField2))
        {
            return htmlField1;
        }

        return htmlField1 + '\n' + htmlField2;
    }
}
