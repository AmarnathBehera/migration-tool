
//namespace BDO.GWT.Core.Services;


//using global::BDO.GWT.Core.Constants;
//using global::BDO.GWT.Core.Data.Queries;
//using global::BDO.GWT.Core.DTOs;
//using global::BDO.GWT.Core.ValueObjects;
//using global::BDO.GWT.Web.Core.DTOs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading;
//using System.Threading.Tasks;


//public class CareerPageCardsService : GenericPageCardsService<Career, CareerCardDto>
//{
//    private readonly ICultureContext _cultureContext;
//    private readonly IMultiTypePageRetriever _multiTypePageRetriever;
//    private readonly IPageRetriever<Career> _careerPageRetriever;
//    private readonly IPageRetriever<LocationOffice> _locationOfficePageRetriever;
//    private readonly IPageUrlService _pageUrlService;
//    private readonly IRetriever<CareerEmploymentType> _careerEmploymentTypeRetriever;
//    private readonly IRetriever<CareerJobTitle> _careerJobTitleRetriever;
//    private readonly IRetriever<CareerLevel> _careerLevelRetriever;

//    public CareerPageCardsService(ICardsService cardsService, ICultureContext cultureContext, IMultiTypePageRetriever multiTypePageRetriever, IPageRetriever<Career> careerPageRetriever, IPageRetriever<LocationOffice> locationOfficePageRetriever, IPageUrlService pageUrlService, IRetriever<CareerEmploymentType> careerEmploymentTypeRetriever, IRetriever<CareerJobTitle> careerJobTitleRetriever, IRetriever<CareerLevel> careerLevelRetriever) : base(cardsService)
//    {
//        _cultureContext = cultureContext;
//        _multiTypePageRetriever = multiTypePageRetriever;
//        _careerPageRetriever = careerPageRetriever;
//        _locationOfficePageRetriever = locationOfficePageRetriever;
//        _pageUrlService = pageUrlService;
//        _careerEmploymentTypeRetriever = careerEmploymentTypeRetriever;
//        _careerJobTitleRetriever = careerJobTitleRetriever;
//        _careerLevelRetriever = careerLevelRetriever;
//    }

//    public override async Task<PaginationDto<CareerCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<IPageQuery<Career>>? decorateQuery, Func<Career, bool> isFeatured, CancellationToken cancellationToken)
//    {
//        var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

//        string? parentPageNodeAliasPath = null;

//        var careerPages = await _careerPageRetriever.RetrievePagesAsync(query =>
//        {
//            ApplyCommonCardFilter(query);
//            foreach (var whereExpression in filterWhereExpressions)
//            {
//                query.Where(whereExpression);
//            }
//            decorateQuery?.Invoke(query);
//            query.Select(x => new
//            {
//                x.ID,
//                x.MetadataTeaserImage,
//                x.MetadataTeaserImageAltText,
//                x.MetadataDescription,
//                x.MetadataTitle,
//                x.CareerEmploymentType,
//                x.CareerReference,
//                x.CareerLevel,
//                x.MetadataOfficeLocation,
//                x.CareerJobTitle,
//                x.NodeGUID,
//                x.NodeID,// Required for URL
//                x.PublishFrom,
//            });

//            if (query.Properties.Path is { Type: PathTypeEnum.Children })
//            {
//                parentPageNodeAliasPath = query.Properties.Path.Value.Path;
//            }

//        }, cancellationToken);

//        var orderedCareerPages = careerPages
//            .OrderByDescending(isFeatured)
//            .ThenByDescending(x => x.PublishFrom)
//            .ToArray();

//        var paginatedCareerPages = PaginationHelper.PaginatePages(orderedCareerPages, cardsQuery)
//            .ToArray();

//        var itemDtos = await ToCareerPageCardListAsync(parentPageNodeAliasPath, paginatedCareerPages, _cultureContext.CurrentLanguageTag, cancellationToken);

//        var paginationDto = new PaginationDto<CareerCardDto>
//        {
//            Data = itemDtos,
//            TotalCount = orderedCareerPages.Length,
//        };

//        return paginationDto;
//    }

//    private async Task<CareerCardDto[]> ToCareerPageCardListAsync(string? parentPageNodeAliasPath, Career[] careerPages, LanguageTag languageTag, CancellationToken cancellationToken)
//    {
//        var parentPageUrl = await GetParentPageUrlAsync(parentPageNodeAliasPath, cancellationToken);

//        var careerEmploymentIds = careerPages.Select(x => x.CareerEmploymentType).Distinct().ToArray();
//        var careerLevelIds = careerPages.Select(x => x.CareerLevel).Distinct().ToArray();
//        var jobTitleIds = careerPages.Select(x => x.CareerJobTitle).Distinct().ToArray();
//        var locationOfficeIds = careerPages.Select(x => x.MetadataOfficeLocation).SelectMany(x => x).Distinct().ToArray();

//        var careerEmploymentTypeById = await RetrieveMetadataEntityNameByIdAsync(_careerEmploymentTypeRetriever, careerEmploymentIds, cancellationToken);
//        var careerLevelById = await RetrieveMetadataEntityNameByIdAsync(_careerLevelRetriever, careerLevelIds, cancellationToken);
//        var careerJobTitleById = await RetrieveMetadataEntityNameByIdAsync(_careerJobTitleRetriever, jobTitleIds, cancellationToken);
//        var locationOfficeNameById = (await _locationOfficePageRetriever.RetrievePagesAsync(q =>
//        {
//            q.Where(x => locationOfficeIds.Contains(x.ID));
//        }, cancellationToken)).ToDictionary(x => x.ID, x => x.MetadataTitle);

//        var relativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(careerPages.Select(x => x.NodeID).ToArray(), languageTag, cancellationToken);

//        return careerPages.Select(career =>
//        {
//            careerJobTitleById.TryGetValue(career.CareerJobTitle, out var careerJobTitle);

//            var careerCardDto = new CareerCardDto
//            {
//                ImageURL = career.MetadataTeaserImage?.ToHtmlCompatibleString(),
//                ImageAltText = career.MetadataTeaserImageAltText,
//                Title = career.MetadataTitle,
//                JobTitle = careerJobTitle?.Name,
//                Reference = career.CareerReference,
//                EmploymentType = careerEmploymentTypeById.TryGetValue(career.CareerEmploymentType, out var careerEmploymentType) ? careerEmploymentType.Name : null,
//                Level = careerLevelById.TryGetValue(career.CareerLevel, out var careerLevel) ? careerLevel.Name : null,
//                Locations = career.MetadataOfficeLocation.Select(id => locationOfficeNameById.GetValueOrDefault(id)).OfType<string>().ToArray(),
//                PublishDate = career.PublishFrom,
//                ApplyURL = relativeUrlByNodeId[career.NodeID].ToHtmlCompatibleString(),
//                JobTitleURL = parentPageUrl is not null && careerJobTitle is not null ? $"{parentPageUrl}?careerJobTitle={careerJobTitle.CodeName}" : null,
//            };

//            return careerCardDto;

//        }).ToArray();
//    }

//    private static async Task<IDictionary<int, TMetadataEntity>> RetrieveMetadataEntityNameByIdAsync<TMetadataEntity>(IRetriever<TMetadataEntity> retriever, IEnumerable<int> ids, CancellationToken cancellationToken)
//        where TMetadataEntity : DomainEntity, IMetadataEntity
//    {
//        var entities = await retriever.RetrieveEntitiesAsync(query =>
//        {
//            query.Where(x => ids.Contains(x.ID));
//            query.Select(x => new
//            {
//                x.ID,
//                x.Name,
//                x.CodeName,
//            });

//        }, cancellationToken);

//        var metadataEntityNamesById = entities.ToDictionary(x => x.ID, x => x);

//        return metadataEntityNamesById;
//    }

//    protected override async Task<Expression<Func<Career, bool>>?> GetPageFilterExpressionAsync(Type filterPageType, string filterParameterName, string filterParameterValue)
//    {
//        var page = await _multiTypePageRetriever.RetrievePageAsync(query =>
//        {
//            query.Where(x => x.Name == filterParameterValue);
//            query.Types(filterPageType);
//        });

//        if (page is null)
//        {
//            return null;
//        }

//        Expression<Func<Career, bool>>? whereExpression = filterParameterName switch
//        {
//            FilterParameterName.OfficeLocation => x => x.MetadataOfficeLocation.Contains(page.NodeID),
//            _ => null,
//        };

//        return whereExpression;
//    }

//    protected override async Task<Expression<Func<Career, bool>>?> GetEntityFilterExpressionAsync(Type filterEntityType, string filterParameterName, string filterParameterValue)
//    {
//        switch (filterParameterName)
//        {
//            case FilterParameterName.CareerEmploymentType:
//                {
//                    var entity = await _careerEmploymentTypeRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

//                    if (entity is not null)
//                    {
//                        return x => x.CareerEmploymentType == entity.ID;
//                    }

//                    break;
//                }

//            case FilterParameterName.CareerJobTitle:
//                {
//                    var entity = await _careerJobTitleRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

//                    if (entity is not null)
//                    {
//                        return x => x.CareerJobTitle == entity.ID;
//                    }

//                    break;
//                }

//            case FilterParameterName.CareerLevel:
//                {
//                    var entity = await _careerLevelRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

//                    if (entity is not null)
//                    {
//                        return x => x.CareerLevel == entity.ID;
//                    }

//                    break;
//                }
//        }

//        return null;
//    }

//    protected override Task<Expression<Func<Career, bool>>?> GetFieldFilterExpressionAsync(Type filterInterfaceType, string filterParameterName, string filterParameterValue)
//    {
//        return Task.FromResult<Expression<Func<Career, bool>>?>(null);
//    }

//    private async Task<string?> GetParentPageUrlAsync(string? parentPageNodeAliasPath, CancellationToken cancellationToken)
//    {
//        if (parentPageNodeAliasPath is null)
//        {
//            return null;
//        }

//        var parentPage = await _multiTypePageRetriever.RetrievePageAsync(query => query.Where(x => x.NodeAliasPath == parentPageNodeAliasPath), cancellationToken);

//        return parentPage is not null ? (await _pageUrlService.GetPageRelativeUrlAsync(parentPage.NodeID, parentPage.LanguageTag, cancellationToken)).ToHtmlCompatibleString() : null;
//    }
//}

