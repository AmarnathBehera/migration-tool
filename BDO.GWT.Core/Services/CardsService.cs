using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using System.Linq.Expressions;

namespace BDO.GWT.Core.Services;

public interface ICardsService
{
    //void AddFilterQueryDecorator<TPageFilter>(Action<IMultiTypePageQuery<Page>> decorateFilterQuery)
    //    where TPageFilter : Page;

    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken);

    //Task<Expression<Func<T, bool>>?> BuildPageGroupFilterExpressionAsync<T>(string nodeAliasPathPrefix, params (Type Type, Expression<Func<T, ICollection<int>>> Property)[] parameters)
    //    where T : Page;
}

public class CardsService : ICardsService
{
    private readonly IContentService _contentService;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    //private readonly Dictionary<Type, Action<IMultiTypePageQuery<Page>>> _pageFilterQueryDecoratorsByFilterPageType = new();
    //private readonly IChildPageHelper _childPageHelper;
    //private readonly ICultureContext _cultureContext;
    //private readonly IMultiTypePageRetriever _multiTypePageRetriever;
    //private readonly IRetriever<BusinessIssue> _businessIssueRetriever;
    //private readonly IRetriever<CareerEmploymentType> _careerEmploymentTypeRetriever;
    //private readonly IRetriever<CareerJobTitle> _careerJobTitleRetriever;
    //private readonly IRetriever<CareerLevel> _careerLevelRetriever;
    //private readonly IRetriever<CareerType> _careerTypeRetriever;
    //private readonly IRetriever<ContentType> _contentTypeRetriever;
    //private readonly IRetriever<Credential> _credentialRetriever;
    //private readonly IRetriever<DealType> _dealTypeRetriever;
    //private readonly IRetriever<EventType> _eventTypeRetriever;
    //private readonly IRetriever<GlobalDealType> _globalDealTypeRetriever;
    //private readonly IRetriever<GlobalIndustry> _globalIndustryRetriever;
    //private readonly IRetriever<GlobalSubIndustry> _globalSubIndustryRetriever;
    //private readonly IRetriever<ProfileGroup> _profileGroupRetriever;
    //private readonly IRetriever<IndustryEntity> _industryEntityRetriever;
    //private readonly IRetriever<SubIndustry> _subIndustryRetriever;

    public CardsService(
        IContentService contentService,
        IWebsiteChannelContext websiteChannelContext
        )
    //IChildPageHelper childPageHelper, ICultureContext cultureContext, IMultiTypePageRetriever multiTypePageRetriever, IRetriever<BusinessIssue> businessIssueRetriever, IRetriever<CareerEmploymentType> careerEmploymentTypeRetriever, IRetriever<CareerJobTitle> careerJobTitleRetriever, IRetriever<CareerLevel> careerLevelRetriever, IRetriever<CareerType> careerTypeRetriever, IRetriever<ContentType> contentTypeRetriever, IRetriever<Credential> credentialRetriever, IRetriever<DealType> dealTypeRetriever, IRetriever<EventType> eventTypeRetriever, IRetriever<GlobalDealType> globalDealTypeRetriever, IRetriever<GlobalIndustry> globalIndustryRetriever, IRetriever<GlobalSubIndustry> globalSubIndustryRetriever, IRetriever<IndustryEntity> industryEntityRetriever, IRetriever<ProfileGroup> profileGroupRetriever, IRetriever<SubIndustry> subIndustryRetriever)
    {
        _contentService = contentService;
        _websiteChannelContext = websiteChannelContext;
        //_childPageHelper = childPageHelper;
        //_cultureContext = cultureContext;
        //_multiTypePageRetriever = multiTypePageRetriever;
        //_businessIssueRetriever = businessIssueRetriever;
        //_careerEmploymentTypeRetriever = careerEmploymentTypeRetriever;
        //_careerJobTitleRetriever = careerJobTitleRetriever;
        //_careerLevelRetriever = careerLevelRetriever;
        //_careerTypeRetriever = careerTypeRetriever;
        //_contentTypeRetriever = contentTypeRetriever;
        //_credentialRetriever = credentialRetriever;
        //_dealTypeRetriever = dealTypeRetriever;
        //_eventTypeRetriever = eventTypeRetriever;
        //_globalDealTypeRetriever = globalDealTypeRetriever;
        //_globalIndustryRetriever = globalIndustryRetriever;
        //_globalSubIndustryRetriever = globalSubIndustryRetriever;
        //_industryEntityRetriever = industryEntityRetriever;
        //_profileGroupRetriever = profileGroupRetriever;
        //_subIndustryRetriever = subIndustryRetriever;
    }

    //public void AddFilterQueryDecorator<TPageFilter>(Action<IMultiTypePageQuery<Page>> decorateFilterQuery)
    //    where TPageFilter : Page
    //{
    //    _pageFilterQueryDecoratorsByFilterPageType.Add(typeof(TPageFilter), decorateFilterQuery);
    //}

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        var filterItems = new List<FilterItemDto<OptionDto>>();

        foreach (var filterParameterName in filterParameterNames)
        {
            var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

            if (filterParameterData is null)
            {
                continue;
            }

            var options = filterParameterData.FilterParameterType switch
            {
                FilterParameterType.Page => await BuildOptionsForPageFilterAsync(filterParameterData, cancellationToken),
               // FilterParameterType.Entity => await BuildOptionsForEntityFilterAsync(filterParameterData, cancellationToken),
               // FilterParameterType.Field => await BuildOptionsForFieldFilterAsync(filterParameterData, filterParameterName, nodeAliasPath, cancellationToken),
                _ => throw new NotSupportedException($"Filter parameter type {filterParameterData.FilterParameterType} is not supported."),
            };

            var filterItem = FilterDtoUtility.CreateFilterItemDto(FilterItemType.Select, filterParameterName, filterParameterData.LiveSiteDisplayName, null, options);

            filterItems.Add(filterItem);
        }

        return filterItems.ToArray();
    }

    //public async Task<Expression<Func<T, bool>>?> BuildPageGroupFilterExpressionAsync<T>(string nodeAliasPathPrefix, params (Type Type, Expression<Func<T, ICollection<int>>> Property)[] parameters)
    //    where T : Page
    //{
    //    var types = parameters.Select(p => p.Type).ToArray();
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType(types, nodeAliasPathPrefix);

    //    var func = new List<Func<T, bool>>();

    //    foreach (var (type, propertyExpression) in parameters)
    //    {
    //        var nodeIds = PageTypeUtility.GetPageTypeNodeIds(type, nodeIdsByPageType);
    //        func.Add(x => propertyExpression.Compile()(x).Intersect(nodeIds).Any());
    //    }

    //    return x => func.Any(f => f(x));
    //}

    private async Task<OptionDto[]> BuildOptionsForPageFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    {
        var entityType = filterParameterData.EntityType;

        var items = await _contentService.GetItemsByTypesAsync(
            new[] { entityType },
            _websiteChannelContext.WebsiteChannelID, 
            cancellationToken);

        var options = items
      .Select(x =>
      {
          var type = x.GetType();

          var metadataTitle =
              type.GetProperty("MetadataTitle")?.GetValue(x) as string;

          var metadataOgTitle =
              type.GetProperty("MetadataOGTitle")?.GetValue(x) as string;

          var systemFields =
              type.GetProperty("SystemFields")?.GetValue(x);

          var contentItemName =
              systemFields?.GetType()
                  .GetProperty("ContentItemName")
                  ?.GetValue(systemFields) as string;

          var name = metadataTitle ?? metadataOgTitle ?? contentItemName;

          return new OptionDto
          {
              Value = name,
              Label = name
          };
      })
      .Where(x => !string.IsNullOrWhiteSpace(x.Value))
      .OrderBy(x => x.Label)
      .ToArray();

        return options;

    }

    //private async Task<OptionDto[]> BuildOptionsForEntityFilterAsync(FilterParameterData filterParameterData, CancellationToken cancellationToken)
    //{
    //    return filterParameterData.EntityType switch
    //    {
    //        { } type when type == typeof(BusinessIssue) => await CreateMetadataEntityOptionsAsync(_businessIssueRetriever, cancellationToken),
    //        { } type when type == typeof(CareerEmploymentType) => await CreateMetadataEntityOptionsAsync(_careerEmploymentTypeRetriever, cancellationToken),
    //        { } type when type == typeof(CareerJobTitle) => await CreateMetadataEntityOptionsAsync(_careerJobTitleRetriever, cancellationToken),
    //        { } type when type == typeof(CareerLevel) => await CreateMetadataEntityOptionsAsync(_careerLevelRetriever, cancellationToken),
    //        { } type when type == typeof(CareerType) => await CreateMetadataEntityOptionsAsync(_careerTypeRetriever, cancellationToken),
    //        { } type when type == typeof(ContentType) => await CreateMetadataEntityOptionsAsync(_contentTypeRetriever, cancellationToken),
    //        { } type when type == typeof(Credential) => await CreateMetadataEntityOptionsAsync(_credentialRetriever, cancellationToken),
    //        { } type when type == typeof(DealType) => await CreateDealCardEntityOptionAsync(_dealTypeRetriever, cancellationToken),
    //        { } type when type == typeof(EventType) => await CreateMetadataEntityOptionsAsync(_eventTypeRetriever, cancellationToken),
    //        { } type when type == typeof(GlobalDealType) => await CreateGlobalDealCardEntityOptionAsync(_globalDealTypeRetriever, cancellationToken),
    //        { } type when type == typeof(GlobalIndustry) => await CreateGlobalDealCardEntityOptionAsync(_globalIndustryRetriever, cancellationToken),
    //        { } type when type == typeof(GlobalSubIndustry) => await CreateGlobalDealCardEntityOptionAsync(_globalSubIndustryRetriever, cancellationToken),
    //        { } type when type == typeof(IndustryEntity) => await CreateDealCardEntityOptionAsync(_industryEntityRetriever, cancellationToken),
    //        { } type when type == typeof(ProfileGroup) => await CreateMetadataEntityOptionsAsync(_profileGroupRetriever, cancellationToken),
    //        { } type when type == typeof(SubIndustry) => await CreateDealCardEntityOptionAsync(_subIndustryRetriever, cancellationToken),
    //        _ => throw new NotSupportedException($"Entity type {filterParameterData.EntityType} is not supported."),
    //    };
    //}

    //private async Task<OptionDto[]> BuildOptionsForFieldFilterAsync(FilterParameterData filterParameterData, string filterParameterName, string? nodeAliasPath, CancellationToken cancellationToken)
    //{
    //    var options = Array.Empty<OptionDto>();
    //    switch (filterParameterData.EntityType)
    //    {
    //        case { } type when type == typeof(IKeywordsPage):
    //            var keywords = await _multiTypePageRetriever.RetrievePagesAsync<IKeywordsPage>(q =>
    //            {
    //                q.Select(x => new
    //                {
    //                    x.MetadataKeywords,
    //                });
    //            }, cancellationToken);

    //            options = keywords
    //                .SelectMany(x => KeywordsHelper.ParseKeywords(x.MetadataKeywords))
    //                .Distinct()
    //                .Select(x => FilterDtoUtility.CreateOptionDto(x, x))
    //                .ToArray();
    //            break;

    //        case { } type when type == typeof(IDealPage) && FilterParameterName.Country == filterParameterName:

    //            var dealsWithCountry = await _multiTypePageRetriever.RetrievePagesAsync<IDealCardPage>(q =>
    //            {
    //                if (nodeAliasPath is not null)
    //                {
    //                    q.Path(nodeAliasPath, PathTypeEnum.Children);
    //                }
    //                q.Select(x => new
    //                {
    //                    x.DealCountry,
    //                });
    //            }, cancellationToken);

    //            options = dealsWithCountry
    //                .Select(x => x.DealCountry)
    //                .Where(x => !string.IsNullOrWhiteSpace(x))
    //                .Distinct()
    //                .OrderBy(x => x)
    //                .Select(x => FilterDtoUtility.CreateOptionDto(x, x))
    //                .ToArray();
    //            break;

    //        case { } type when type == typeof(IDealPage) && FilterParameterName.Year == filterParameterName:
    //            var dealsWithYear = await _multiTypePageRetriever.RetrievePagesAsync<IDealCardPage>(q =>
    //            {
    //                if (nodeAliasPath is not null)
    //                {
    //                    q.Path(nodeAliasPath, PathTypeEnum.Children);
    //                }
    //                q.Select(x => new
    //                {
    //                    x.DateField,
    //                });
    //            }, cancellationToken);

    //            options = dealsWithYear
    //                .Select(x => x.DateField.Year)
    //                .Distinct()
    //                .OrderByDescending(x => x)
    //                .Select(x => FilterDtoUtility.CreateOptionDto(x.ToString(), x.ToString()))
    //                .ToArray();
    //            break;
    //    }

    //    return options;
    //}

    //private async Task<OptionDto[]> CreateMetadataEntityOptionsAsync<TEntity>(IRetriever<TEntity> retriever, CancellationToken cancellationToken) where TEntity : DomainEntity, IMetadataEntity
    //{
    //    var entities = await retriever.RetrieveEntitiesAsync(q =>
    //    {
    //        q.Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag);
    //        q.Select(x => new
    //        {
    //            x.Name,
    //            x.CodeName,
    //        });
    //    }, cancellationToken);

    //    var options = entities
    //        .OrderBy(x => x.Name)
    //        .Select(x => FilterDtoUtility.CreateOptionDto(x.Name, x.CodeName))
    //        .ToArray();

    //    return options;
    //}

    //private async Task<OptionDto[]> CreateDealCardEntityOptionAsync<TEntity>(IRetriever<TEntity> retriever, CancellationToken cancellationToken) where TEntity : DomainEntity, IDealCardEntity
    //{
    //    var entities = await retriever.RetrieveEntitiesAsync(q =>
    //    {
    //        q.Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag);
    //        q.Select(x => new
    //        {
    //            x.Name,
    //            x.CodeName,
    //        });
    //    }, cancellationToken);

    //    var options = entities
    //        .OrderBy(x => x.Name)
    //        .Select(x => FilterDtoUtility.CreateOptionDto(x.Name, x.CodeName))
    //        .ToArray();

    //    return options;
    //}

    //private static async Task<OptionDto[]> CreateGlobalDealCardEntityOptionAsync<TEntity>(IRetriever<TEntity> retriever, CancellationToken cancellationToken) where TEntity : DomainEntity, IGlobalDealCardEntity
    //{
    //    var entities = await retriever.RetrieveEntitiesAsync(q =>
    //    {
    //        q.Select(x => new
    //        {
    //            x.Name,
    //            x.CodeName,
    //        });
    //    }, cancellationToken);

    //    var options = entities
    //        .OrderBy(x => x.Name)
    //        .Select(x => FilterDtoUtility.CreateOptionDto(x.Name, x.CodeName))
    //        .ToArray();

    //    return options;
    //}
}
