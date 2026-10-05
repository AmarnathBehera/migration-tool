using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IAuthoredContentService
{
    Task<AuthoredContentDto?> GetAuthoredContentDtoAsync(Person person, CancellationToken cancellationToken);
}

public class AuthoredContentService : IAuthoredContentService
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;

    public AuthoredContentService(
        IContentService contentService,
        ICultureContext cultureContext)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
    }

    public async Task<AuthoredContentDto?> GetAuthoredContentDtoAsync(Person person, CancellationToken cancellationToken)
    {
        var parent = (await _contentService.GetPagesAsync<SectionPeoplePage>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 3,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query
                .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                .Where(w => w.WhereEquals("WebPageItemID", person.SystemFields.WebPageItemParentID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        )).FirstOrDefault();

        if (parent is null)
        {
           var blogParent = (await _contentService.GetPagesAsync<BlogContributors>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 3,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query
                .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                .Where(w => w.WhereEquals("WebPageItemID", person.SystemFields.WebPageItemParentID)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        )).FirstOrDefault();

            if (blogParent is not { ShowAuthoredBlogs: true })
            {
                return null;
            }
            var contentPageBlogCards = await GetPersonAuthoredBlogPosts(person, cancellationToken);

            return new AuthoredContentDto
            {
                Title = blogParent.AuthoredBlogTitle,
                Description = blogParent.AuthoredBlogDescription,
                BackgroundImageURL = blogParent.AuthoredBlogsBackgroundImageUrl?.FirstOrDefault()?.ContentAttachmentAsset.Url,
                Items = contentPageBlogCards,
            };
        }
        if (parent is not { ShowAuthoredInsights: true })
        {
            return null;
        }

        var contentPageCards = await GetPersonAuthoredContentAsync(parent, person, cancellationToken);

        return new AuthoredContentDto
        {
            Title = parent.AuthoredInsightsTitle,
            Description = parent.AuthoredInsightsDescription,
            BackgroundImageURL = parent.AuthoredInsightsBackgroundImageUrl?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            Items = contentPageCards,
        };
    }

    private async Task<AuthoredContentCardDto[]> GetPersonAuthoredBlogPosts(Person person, CancellationToken cancellationToken)
    {
        var results = new[] {await GetPersonAuthoredBlogPostCardsAsync(person, cancellationToken) };

        var authoredBlogCards = results
            .SelectMany(cards => cards)
            .ToArray();

        var sortedAuthoredBlogCards = authoredBlogCards.OrderByDescending(x => x.Date).ToArray();

        return sortedAuthoredBlogCards;
    }
    private async Task<AuthoredContentCardDto[]> GetPersonAuthoredContentAsync(
        SectionPeoplePage sectionPeoplePage,
        Person person,
        CancellationToken cancellationToken)
    {
        var displayedTypes = sectionPeoplePage.DisplayedAuthoredContentPageTypes;

        var tasks = new List<Task<AuthoredContentCardDto[]>>();

        if (displayedTypes.Contains(nameof(NewsArticle)))
            tasks.Add(GetPersonAuthoredNewsArticleCardsAsync(person, cancellationToken));

        if (displayedTypes.Contains(nameof(Deal)))
            tasks.Add(GetPersonAuthoredDealCardsAsync(person, cancellationToken));

        if (displayedTypes.Contains(nameof(MicrositeMasterPage)))
            tasks.Add(GetPersonAuthoredMicrositeMasterPageCardsAsync(person));

        if (displayedTypes.Contains(nameof(MicrositePage)))
            tasks.Add(GetPersonAuthoredMicrositePageCardsAsync(person));

        if (displayedTypes.Contains(nameof(BlogPost)))
            tasks.Add(GetPersonAuthoredBlogPostCardsAsync(person, cancellationToken));

        if (displayedTypes.Contains(nameof(Insight)))
            tasks.Add(GetPersonAuthoredInsightCardsAsync(person, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(cards => cards)
            .OrderByDescending(card => card.Date)
            .ToArray();
    }
    private static Task<AuthoredContentCardDto[]> GetPersonAuthoredMicrositeMasterPageCardsAsync(Person person)
    {
        var pages = person.MicrositeMasterPagesPerson?.ToArray()
            ?? Array.Empty<MicrositeMasterPage>();

        var cards = pages
            .Select(p => MapToCard(
                contentType: "micrositeMasterPage",
                imageUrl: p.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                imageAltText: p.MetadataTeaserImageAltText,
                date: p.SystemFields.ContentItemCommonDataLastPublishedWhen
                              ?? p.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                title: p.MetadataTitle,
                description: p.MetadataDescription,
                url: p.GetUrl().RelativePath))
            .ToArray();

        return Task.FromResult(cards);
    }

    private static Task<AuthoredContentCardDto[]> GetPersonAuthoredMicrositePageCardsAsync(Person person)
    {
        var pages = person.MicrositePagesPerson?.ToArray()
            ?? Array.Empty<MicrositePage>();

        var cards = pages
            .Select(p => MapToCard(
                contentType: "micrositePage",
                imageUrl: p.MicrositeImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                imageAltText: p.MicrositeImageAltText,
                date: p.SystemFields.ContentItemCommonDataLastPublishedWhen
                              ?? p.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                title: p.MetadataTitle,
                description: p.MetadataDescription,
                url: p.GetUrl().RelativePath))
            .ToArray();

        return Task.FromResult(cards);
    }
    private async Task<AuthoredContentCardDto[]> GetPersonAuthoredNewsArticleCardsAsync(
        Person person,
        CancellationToken cancellationToken)
    {
        var personGuid = person.SystemFields.ContentItemGUID.ToString();

        var pages = (await _contentService.GetPagesAsync<NewsArticle>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query.Where(w => w.WhereContains(nameof(NewsArticle.RelevantContactPerson), personGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToArray() ?? Array.Empty<NewsArticle>();

        return pages
            .Select(p => MapToCard(
                contentType: "newsArticle",
                imageUrl: p.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                imageAltText: p.MetadataTeaserImageAltText,
                date: p.SystemFields.ContentItemCommonDataLastPublishedWhen
                              ?? p.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                title: p.MetadataTitle,
                description: p.MetadataDescription,
                url: p.GetUrl().RelativePath))
            .ToArray();
    }

    private async Task<AuthoredContentCardDto[]> GetPersonAuthoredDealCardsAsync(
        Person person,
        CancellationToken cancellationToken)
    {
        var personGuid = person.SystemFields.ContentItemGUID.ToString();

        var pages = (await _contentService.GetPagesAsync<Deal>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query.Where(w => w.WhereContains(nameof(Deal.RelevantContactPersonDeal), personGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToArray() ?? Array.Empty<Deal>();

        return pages
            .Select(p => MapToCard(
                contentType: "deal",
                imageUrl: p.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                imageAltText: p.MetadataTeaserImageAltText,
                date: p.SystemFields.ContentItemCommonDataLastPublishedWhen
                              ?? p.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                title: p.MetadataTitle,
                description: p.MetadataDescription,
                url: p.GetUrl().RelativePath))
            .ToArray();
    }

    private async Task<AuthoredContentCardDto[]> GetPersonAuthoredBlogPostCardsAsync(
        Person person,
        CancellationToken cancellationToken)
    {
        var personGuid = person.SystemFields.ContentItemGUID.ToString();

        var pages = (await _contentService.GetPagesAsync<BlogPost>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query.Where(w => w.WhereContains(nameof(BlogPost.BlogRelevantContactPerson), personGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToArray() ?? Array.Empty<BlogPost>();

        return pages
            .Select(p => MapToCard(
                contentType: "blogPost",
                imageUrl: p.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                imageAltText: p.MetadataTeaserImageAltText,
                date: p.SystemFields.ContentItemCommonDataLastPublishedWhen
                              ?? p.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                title: p.MetadataTitle,
                description: p.MetadataDescription,
                url: p.GetUrl().RelativePath))
            .ToArray();
    }
    private async Task<AuthoredContentCardDto[]> GetPersonAuthoredInsightCardsAsync(
        Person person,
        CancellationToken cancellationToken)
    {
        var personGuid = person.SystemFields.ContentItemGUID.ToString();

        var pages = (await _contentService.GetPagesAsync<Insight>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                LanguageName = _cultureContext.CurrentLanguageTag,
                UseLanguageFallbacks = false,
            },
            query => query.Where(w => w.WhereContains(nameof(Insight.RelevantContactPerson), personGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToArray() ?? Array.Empty<Insight>();

        return pages
            .Select(p => MapToCard(
                contentType: "insight",
                imageUrl: p.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                imageAltText: p.MetadataTeaserImageAltText,
                date: p.SystemFields.ContentItemCommonDataLastPublishedWhen
                              ?? p.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                title: p.MetadataTitle,
                description: p.MetadataDescription,
                url: p.GetUrl().RelativePath))
            .ToArray();
    }
    private static AuthoredContentCardDto MapToCard(
        string contentType,
        string? imageUrl,
        string? imageAltText,
        DateTime? date,
        string? title,
        string? description,
        string? url) =>
        new()
        {
            ContentType = contentType,
            ImageURL = imageUrl?.TrimStart('~'),
            ImageAltText = imageAltText,
            IsFeatured = false,
            Type = null,
            Date = date,
            Title = title ?? string.Empty,
            Description = HtmlHelper.ExtractTextFromHtml(description),
            ReadMoreLink = new LinkItemDto
            {
                Title = ResourceStringKey.LiveSite.ReadMore,
                URL = url ?? string.Empty,
            },
        };
}