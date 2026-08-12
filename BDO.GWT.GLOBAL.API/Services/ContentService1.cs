using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IContentService1
{
    Task<IEnumerable<ContentCategoryDto>> GetContentCategoriesAsync(string culture, CancellationToken cancellationToken);
    
}

public class ContentService1 : IContentService1
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IValidationState _validationState;

    public ContentService1(IContentQueryExecutor contentQueryExecutor, IValidationState validationState)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _validationState = validationState;
    }

    public async Task<IEnumerable<ContentCategoryDto>> GetContentCategoriesAsync(string culture, CancellationToken cancellationToken)
    {
        var languageTag = ValidateAndNormalizeCulture(culture);

        var categories = new List<ContentCategoryDto>
        {
            await GetBusinessIssueCategoryDtoAsync(languageTag, cancellationToken),
            await GetContentCategoryDtoAsync("Business Line", "BusinessLine", BusinessLine.CONTENT_TYPE_NAME, languageTag, cancellationToken),
            await GetContentCategoryDtoAsync("Service Area", "ServiceArea", ServiceArea.CONTENT_TYPE_NAME, languageTag, cancellationToken),
            await GetContentCategoryDtoAsync("Industry", "Industry", Industry.CONTENT_TYPE_NAME, languageTag, cancellationToken),
        };

        return categories;
    }

    private LanguageTag ValidateAndNormalizeCulture(string languageTagString)
    {
        if (LanguageTag.TryParse(languageTagString, out var languageTag))
        {
            return languageTag;
        }

        _validationState.AddError("culture", $"Incorrect language tag '{languageTagString}' format.");
        throw new ValidationException();
    }

    private async Task<ContentCategoryDto> GetBusinessIssueCategoryDtoAsync(LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(Businessissue.CONTENT_TYPE_NAME, config => config
                .Columns(nameof(Businessissue.SystemFields.ContentItemID), nameof(Businessissue.Name)))
            .InLanguage(languageTag.ToString())
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
     string name, string codeName, string contentTypeName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, config => config
                .Columns("ContentItemID", "MetadataTitle"))
            .InLanguage(languageTag.ToString())
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