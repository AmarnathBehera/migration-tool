using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IInFocusService
{
    Task<InFocusDto> GetInFocusAsync(InFocusData data, CancellationToken cancellationToken);
}

public class InFocusService : IInFocusService
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;

    public InFocusService(IContentService contentService, ICultureContext cultureContext)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
    }

    public async Task<InFocusDto> GetInFocusAsync(InFocusData data, CancellationToken cancellationToken)
    {
        var topicsDtos = await GetInFocusTopicDtosAsync(data.PageGuids, cancellationToken);

        return new InFocusDto
        {
            Title = data.Title,
            Topics = topicsDtos,
            Subtitle = data.Subtitle,
            ImageURL = data.ImageURL,
            ImageAltText = data.ImageAltText,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
        };
    }

    private async Task<InFocusTopicDto[]> GetInFocusTopicDtosAsync(IEnumerable<Guid> pageGuids, CancellationToken cancellationToken)
    {
        var pagesList = await _contentService.GetPagesAsync<InFocusPage>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false
            },
            query => query.Where(w => w.WhereIn("ContentItemGUID", pageGuids)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var pages = pagesList.OrderBy(o => Array.IndexOf(pageGuids.ToArray(), o.SystemFields.ContentItemGUID));

        var result = pages.Select(inFocusPage => new InFocusTopicDto
        {
            Title = inFocusPage.MetadataTitle,
            Items = GetInFocusItems(inFocusPage),
        })
            .ToList();

        return [.. result];
    }

    private static InFocusItemDto[] GetInFocusItems(InFocusPage inFocusPage)
    {
        var items = new List<InFocusItemDto>();

        var titles = new[]
        {
            inFocusPage.TitleLink1,
            inFocusPage.TitleLink2,
            inFocusPage.TitleLink3,
        };

        var summaries = new[]
        {
            inFocusPage.SummaryLink1,
            inFocusPage.SummaryLink2,
            inFocusPage.SummaryLink3,
        };

        var links = new[]
        {
            inFocusPage.SelectorLink1,
            inFocusPage.SelectorLink2,
            inFocusPage.SelectorLink3,
        };

        var linkTexts = new[]
        {
            inFocusPage.TextLink1,
            inFocusPage.TextLink2,
            inFocusPage.TextLink3,
        };

        var openInNewTabs = new[]
        {
            inFocusPage.NewTabLink1,
            inFocusPage.NewTabLink2,
            inFocusPage.NewTabLink3,
        };

        for (var i = 0; i < titles.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(titles[i]) && string.IsNullOrWhiteSpace(summaries[i]) && links[i] is null)
            {
                continue;
            }

            items.Add(new InFocusItemDto
            {
                Title = titles[i],
                Summary = summaries[i],
                Link = links[i],
                LinkText = linkTexts[i],
                OpenInNewTab = openInNewTabs[i],
            });
        }

        return [.. items];
    }
}
