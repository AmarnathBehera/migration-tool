
using BDO.GWT.Core.Entities;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Helper;

public interface ICarouselHelper
{
    Task<Action<ContentTypeQueryParameters>> CreateLocationCityRelatedContentFilterAsync(
        string treePath,
        string channelName,
        CancellationToken cancellationToken);
}

public class CarouselHelper : ICarouselHelper
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public CarouselHelper(
        IContentQueryExecutor contentQueryExecutor,
        IWebsiteChannelContext websiteChannelContext)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<Action<ContentTypeQueryParameters>> CreateLocationCityRelatedContentFilterAsync(
        string treePath,
        string channelName,
        CancellationToken cancellationToken)
    {
        var queryBuilder = new ContentItemQueryBuilder()
        .ForContentType(LocationOffice.CONTENT_TYPE_NAME, parameters => parameters
        .ForWebsite(channelName, PathMatch.Children(treePath))
        .Columns(nameof(WebPageFields.ContentItemID)));

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var locationOffices = await _contentQueryExecutor
            .GetWebPageResult(queryBuilder, row => row.ContentItemID, executionOptions, cancellationToken);

        var locationOfficeIds = locationOffices.ToList();

        return parameters => parameters
            .Where(w => w.WhereIn(
                nameof(IOfficeTaggablePage.MetadataOfficeLocation),
                locationOfficeIds));
    }
}