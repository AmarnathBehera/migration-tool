using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Services;

public interface IMicrositeMasterPageService
{
    Task<MicrositeMasterPageDto> GetMicrositeMasterPageDtoAsync(CancellationToken cancellationToken);
}

public class MicrositeMasterPageService : IMicrositeMasterPageService
{
    private readonly IContentService _contentService;

    public MicrositeMasterPageService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<MicrositeMasterPageDto> GetMicrositeMasterPageDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<MicrositeMasterPage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (page is null)
        {
            return new MicrositeMasterPageDto();
        }

        return new MicrositeMasterPageDto
        {
            HeadHtml = page.HeadHTML,
            BodyTopHtml = page.BodyTopHTML,
            BodyBottomHtml = page.BodyBottomHTML,
            CustomCssLinks = page.CustomCSS?
                .Where(a => a.ContentAttachmentAsset?.Url is not null)
                .Select(a => a.ContentAttachmentAsset.Url!)
                ?? Enumerable.Empty<string>(),
            CustomJsLinks = page.CustomJS?
                .Where(a => a.ContentAttachmentAsset?.Url is not null)
                .Select(a => a.ContentAttachmentAsset.Url!)
                ?? Enumerable.Empty<string>(),
        };
    }
}