using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Extensions;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;               
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IContentService1
{
    Task<ActionResult<IEnumerable<ContentCategoryDto>>> GetContentCategoriesAsync(int channelId, string culture, CancellationToken cancellationToken);
}

public class ContentService1 : IContentService1
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IMappingHelper _mappingHelper;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider; // resolve ChannelName from channelId
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;

    public ContentService1(
        IContentQueryExecutor contentQueryExecutor,
        IMappingHelper mappingHelper,
        IInfoProvider<ChannelInfo> channelInfoProvider)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _mappingHelper = mappingHelper;
        _channelInfoProvider = channelInfoProvider;
    }

    public async Task<ActionResult<IEnumerable<ContentCategoryDto>>> GetContentCategoriesAsync(int channelId, string culture, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await _channelInfoProvider.GetAsync(channelId, cancellationToken);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            var categories = new List<ContentCategoryDto>
            {
                await GetBusinessIssueCategoryDtoAsync(channel.ChannelName, currentCulture, cancellationToken),
                await GetContentCategoryDtoAsync("Business Line", "BusinessLine", BusinessLine.CONTENT_TYPE_NAME, channel.ChannelName, currentCulture, cancellationToken),
                await GetContentCategoryDtoAsync("Service Area", "ServiceArea", ServiceArea.CONTENT_TYPE_NAME, channel.ChannelName, currentCulture, cancellationToken),
                await GetContentCategoryDtoAsync("Industry", "Industry", Industry.CONTENT_TYPE_NAME, channel.ChannelName, currentCulture, cancellationToken),
            };

            return categories;
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult("One or more validation errors occurred.");
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    private async Task<ContentCategoryDto> GetBusinessIssueCategoryDtoAsync(string channelName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(Businessissue.CONTENT_TYPE_NAME, config => config
                .ForWebsite(channelName)   // <-- scopes the query to the requested channel
                .Columns(nameof(Businessissue.SystemFields.ContentItemID), nameof(Businessissue.Name)))
            .InLanguage(languageTag.Locale)
            .Parameters(q => q.OrderBy(new OrderByColumn(nameof(Businessissue.Name))));

        var businessIssues = await _contentQueryExecutor.GetMappedResult<Businessissue>(
            builder, cancellationToken: cancellationToken);

        return new ContentCategoryDto
        {
            Name = "Business Issue",
            Code = "BusinessIssue",
            SubCategories = businessIssues
                .Select(businessIssue => new ContentSubCategoryDto
                {
                    Id = businessIssue.SystemFields.ContentItemID,
                    Name = businessIssue.Name,
                })
                .ToList(),
        };
    }

    private async Task<ContentCategoryDto> GetContentCategoryDtoAsync(
        string name, string codeName, string contentTypeName, string channelName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, config => config
                .ForWebsite(channelName)   // <-- scopes the query to the requested channel
                .Columns("ContentItemID", "MetadataTitle"))
            .InLanguage(languageTag.Locale)
            .Parameters(q => q.OrderBy(new OrderByColumn("MetadataTitle")));

        var pages = await _contentQueryExecutor.GetResult(builder, row => new
        {
            ContentItemID = row.GetValue<int>("ContentItemID"),
            MetadataTitle = row.GetValue<string>("MetadataTitle"),
        }, cancellationToken: cancellationToken);

        return new ContentCategoryDto
        {
            Name = name,
            Code = codeName,
            SubCategories = pages
                .Select(page => new ContentSubCategoryDto
                {
                    Id = page.ContentItemID,
                    Name = page.MetadataTitle,
                })
                .ToList(),
        };
    }
}