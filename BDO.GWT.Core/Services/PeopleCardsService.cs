using BDO.GWT.Core.DTOs;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface IPeopleCardsService
{ 
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken);
    //Task<PaginationDto<PersonCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<IPageQuery<Person>>? decorateQuery, Func<Person, bool> isFeatured, CancellationToken cancellationToken);
}
public class PeopleCardsService : IPeopleCardsService// : IPageCardsService<Person, PersonCardDto>
{
    private readonly ICardsService _cardsService;
    //private readonly IChildPageHelper _childPageHelper;
    //private readonly ICultureContext _cultureContext;
    //private readonly IMultiTypePageRetriever _multiTypePageRetriever;
    //private readonly IPageRetriever<Person> _personPageRetriever;
    //private readonly IPageRetriever<SectionPeoplePage> _sectionPeoplePageRetriever;
    //private readonly IPageUrlService _pageUrlService;
    //private readonly IRetriever<Credential> _credentialRetriever;
    //private readonly IRetriever<ProfileGroup> _profileGroupRetriever;
    //private readonly ISettingsService _settingsService;

    public PeopleCardsService(
        ICardsService cardsService
        //IChildPageHelper childPageHelper, ICultureContext cultureContext, IMultiTypePageRetriever multiTypePageRetriever, IPageRetriever<Person> personPageRetriever, IPageRetriever<SectionPeoplePage> sectionPeoplePageRetriever, IPageUrlService pageUrlService, IRetriever<Credential> credentialRetriever, IRetriever<ProfileGroup> profileGroupRetriever, ISettingsService settingsService)
   ) {
        _cardsService = cardsService;
        //_childPageHelper = childPageHelper;
        //_cultureContext = cultureContext;
        //_multiTypePageRetriever = multiTypePageRetriever;
        //_personPageRetriever = personPageRetriever;
        //_sectionPeoplePageRetriever = sectionPeoplePageRetriever;
        //_pageUrlService = pageUrlService;
        //_credentialRetriever = credentialRetriever;
        //_profileGroupRetriever = profileGroupRetriever;
        //_settingsService = settingsService;
    }

    //public void AddFilterQueryDecorator<TPageFilter>(Action<IMultiTypePageQuery<Page>> decorateFilterQuery)
    //    where TPageFilter : Page
    //{
    //    _cardsService.AddFilterQueryDecorator<TPageFilter>(decorateFilterQuery);
    //}

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, nodeAliasPath, cancellationToken);
    }

    //public async Task<PaginationDto<PersonCardDto>> GetFilteredItemsAsync(CardsQuery cardsQuery, Action<IPageQuery<Person>>? decorateQuery, Func<Person, bool> isFeatured, CancellationToken cancellationToken)
    //{
    //    var filterWhereExpressions = await GetFilterParameterWhereExpressionsAsync(cardsQuery.Filters);

    //    var order = cardsQuery.Order;

    //    if (order == null)
    //    {
    //        var peopleSections = await _sectionPeoplePageRetriever.RetrievePagesAsync(query => query.Take(1), cancellationToken);

    //        order = peopleSections.FirstOrDefault()?.PeopleListDefaultOrder ?? "asc";
    //    }

    //    var collation = CultureToCollationMapper.GetCollationForCulture(_cultureContext.CurrentLanguageTag.ToString());

    //    var pages = (await _personPageRetriever.RetrievePagesAsync(query =>
    //    {
    //        ApplyCommonFilter(query);
    //        foreach (var whereExpression in filterWhereExpressions)
    //        {
    //            query.Where(whereExpression);
    //        }
    //        decorateQuery?.Invoke(query);
    //        query.OrderBy(list =>
    //        {
    //            var normalizedOrder = order.ToLower();
    //            switch (normalizedOrder)
    //            {
    //                case "desc":
    //                    list.Descending(nameof(Person.PersonLastName), collation);
    //                    break;

    //                case "asc":
    //                    list.Ascending(nameof(Person.PersonLastName), collation);
    //                    break;

    //                default:
    //                    list.Ascending(nameof(Person.NodeOrder));
    //                    break;
    //            }
    //        });
    //        query.Select(x => new
    //        {
    //            x.ID,
    //            x.MetadataTeaserImage,
    //            x.MetadataTeaserImageAltText,
    //            x.MetadataTitle,
    //            x.NodeID,
    //            x.PersonFirstName,
    //            x.PersonJobTitle,
    //            x.PersonLastName,
    //            x.PersonSurnamePrefixes,
    //        });
    //    }, cancellationToken)).ToArray();

    //    var paginatedPages = PaginationHelper.PaginatePages(pages, cardsQuery)
    //        .ToArray();

    //    var cardDtos = await CreatePeopleCardDtosAsync(paginatedPages, cancellationToken);

    //    var paginationDto = new PaginationDto<PersonCardDto>
    //    {
    //        Data = cardDtos,
    //        TotalCount = pages.Length,
    //    };

    //    return paginationDto;
    //}

    //private static void ApplyCommonFilter(IPageQuery<Person> pageQuery)
    //{
    //    pageQuery
    //        // TODO: Change to page.IsSearchExcluded != true when we implement IS NOT NULL check for DB fields that can be null
    //        .Where(x => x.IsSearchExcluded == false || x.IsSearchExcluded == null);
    //}

    //private async Task<List<Expression<Func<Person, bool>>>> GetFilterParameterWhereExpressionsAsync(IDictionary<string, string> filterParameterValueByName)
    //{
    //    var whereExpressions = new List<Expression<Func<Person, bool>>>();

    //    foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
    //    {
    //        var filterParameterData = FilterUtility.GetFilterParameterData(filterParameterName);

    //        if (filterParameterData is null)
    //        {
    //            continue;
    //        }

    //        if (filterParameterData.FilterParameterType == FilterParameterType.Page)
    //        {
    //            var page = await _multiTypePageRetriever.RetrievePageAsync(query => query
    //                .Types(filterParameterData.EntityType)
    //                .Where(x => x.Name == filterParameterValue)
    //            );

    //            if (page is null)
    //            {
    //                continue;
    //            }

    //            var whereExpression = filterParameterName switch
    //            {
    //                FilterParameterName.BusinessLine => await BuildBusinessLineWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.ServiceArea => await BuildServiceAreaWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.Service => await BuildServiceWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.SubService => x => x.MetadataSubServices.Contains(page.NodeID),
    //                FilterParameterName.IndustryCategory => await BuildIndustryCategoryWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.IndustryPage => await BuildIndustryWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.IndustryService => x => x.MetadataIndustryServices.Contains(page.NodeID),
    //                FilterParameterName.SpecialtyCategory => await BuildSpecialtyCategoryWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.SpecialtyArea => await BuildSpecialtyAreaWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.Specialty => x => x.MetadataSpecialtiesPages.Contains(page.NodeID),
    //                FilterParameterName.MicrositeMasterPage => await BuildMicrositeMasterPageWhereExpression(page.NodeAliasPath),
    //                FilterParameterName.OfficeLocation => x => x.MetadataOfficeLocation.Contains(page.NodeID),
    //                FilterParameterName.City => x => x.MetadataCity.Contains(page.NodeID),
    //                _ => null,
    //            };

    //            if (whereExpression is not null)
    //            {
    //                whereExpressions.Add(whereExpression);
    //            }
    //        }
    //        else if (filterParameterData.FilterParameterType == FilterParameterType.Entity)
    //        {
    //            switch (filterParameterName)
    //            {
    //                case FilterParameterName.Credential:
    //                    {
    //                        var entity = await _credentialRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

    //                        if (entity is not null)
    //                        {
    //                            whereExpressions.Add(x => x.PersonCredentials.Contains(entity.ID));
    //                        }

    //                        break;
    //                    }

    //                case FilterParameterName.ProfileGroup:
    //                    {
    //                        var entity = await _profileGroupRetriever.RetrieveEntityAsync(query => query.Where(x => x.CodeName == filterParameterValue));

    //                        if (entity is not null)
    //                        {
    //                            whereExpressions.Add(x => x.MetadataProfileGroups.Contains(entity.ID));
    //                        }

    //                        break;
    //                    }
    //            }
    //        }
    //    }

    //    return whereExpressions;
    //}

    //private async Task<Expression<Func<Person, bool>>?> BuildBusinessLineWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(BusinessLine),
    //        typeof(ServiceArea),
    //        typeof(Service),
    //        typeof(SubService),
    //    ], nodeAliasPathPrefix);

    //    var businessLineNodeIds = PageTypeUtility.GetPageTypeNodeIds<BusinessLine>(nodeIdsByPageType);
    //    var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
    //    var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);
    //    var subServiceNodeIds = PageTypeUtility.GetPageTypeNodeIds<SubService>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataBusinessLines.Intersect(businessLineNodeIds).Any() ||
    //        x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
    //        x.MetadataServices.Intersect(serviceNodeIds).Any() ||
    //        x.MetadataSubServices.Intersect(subServiceNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>> BuildServiceAreaWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(ServiceArea),
    //        typeof(Service),
    //        typeof(SubService),
    //    ], nodeAliasPathPrefix);

    //    var serviceAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<ServiceArea>(nodeIdsByPageType);
    //    var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);
    //    var subServiceNodeIds = PageTypeUtility.GetPageTypeNodeIds<SubService>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataServiceAreas.Intersect(serviceAreaNodeIds).Any() ||
    //        x.MetadataServices.Intersect(serviceNodeIds).Any() ||
    //        x.MetadataSubServices.Intersect(subServiceNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>> BuildServiceWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(Service),
    //        typeof(SubService),
    //    ], nodeAliasPathPrefix);

    //    var serviceNodeIds = PageTypeUtility.GetPageTypeNodeIds<Service>(nodeIdsByPageType);
    //    var subServiceNodeIds = PageTypeUtility.GetPageTypeNodeIds<SubService>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataServices.Intersect(serviceNodeIds).Any() ||
    //        x.MetadataSubServices.Intersect(subServiceNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>?> BuildIndustryCategoryWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(IndustryCategory),
    //        typeof(Industry),
    //        typeof(IndustryService),
    //    ], nodeAliasPathPrefix);

    //    var industryCategoryNodeIds = PageTypeUtility.GetPageTypeNodeIds<IndustryCategory>(nodeIdsByPageType);
    //    var industryNodeIds = PageTypeUtility.GetPageTypeNodeIds<Industry>(nodeIdsByPageType);
    //    var industryServiceNodeIds = PageTypeUtility.GetPageTypeNodeIds<IndustryService>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataIndustryCategories.Intersect(industryCategoryNodeIds).Any() ||
    //        x.MetadataIndustries.Intersect(industryNodeIds).Any() ||
    //        x.MetadataIndustryServices.Intersect(industryServiceNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>?> BuildIndustryWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(Industry),
    //        typeof(IndustryService),
    //    ], nodeAliasPathPrefix);

    //    var industryNodeIds = PageTypeUtility.GetPageTypeNodeIds<Industry>(nodeIdsByPageType);
    //    var industryServiceNodeIds = PageTypeUtility.GetPageTypeNodeIds<IndustryService>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataIndustries.Intersect(industryNodeIds).Any() ||
    //        x.MetadataIndustryServices.Intersect(industryServiceNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>?> BuildSpecialtyCategoryWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(SpecialtiesCategory),
    //        typeof(SpecialtiesArea),
    //        typeof(SpecialtiesPage),
    //    ], nodeAliasPathPrefix);

    //    var specialtyCategoryNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesCategory>(nodeIdsByPageType);
    //    var specialtyAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesArea>(nodeIdsByPageType);
    //    var specialtyNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesPage>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataSpecialtiesCategories.Intersect(specialtyCategoryNodeIds).Any() ||
    //        x.MetadataSpecialtiesAreas.Intersect(specialtyAreaNodeIds).Any() ||
    //        x.MetadataSpecialtiesPages.Intersect(specialtyNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>?> BuildSpecialtyAreaWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(SpecialtiesArea),
    //        typeof(SpecialtiesPage),
    //    ], nodeAliasPathPrefix);

    //    var specialtyAreaNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesArea>(nodeIdsByPageType);
    //    var specialtyNodeIds = PageTypeUtility.GetPageTypeNodeIds<SpecialtiesPage>(nodeIdsByPageType);

    //    return x =>
    //        x.MetadataSpecialtiesAreas.Intersect(specialtyAreaNodeIds).Any() ||
    //        x.MetadataSpecialtiesPages.Intersect(specialtyNodeIds).Any();
    //}

    //private async Task<Expression<Func<Person, bool>>?> BuildMicrositeMasterPageWhereExpression(string nodeAliasPathPrefix)
    //{
    //    var nodeIdsByPageType = await _childPageHelper.GetDescendantNodeIdsByPageType([
    //        typeof(MicrositeMasterPage),
    //        typeof(MicrositePage),
    //    ], nodeAliasPathPrefix);

    //    var micrositeMasterPageNodeIds = PageTypeUtility.GetPageTypeNodeIds<MicrositeMasterPage>(nodeIdsByPageType);
    //    var micrositePageNodeIds = PageTypeUtility.GetPageTypeNodeIds<MicrositePage>(nodeIdsByPageType);

    //    return x =>
    //        x.MicrositeMasterPages.Intersect(micrositeMasterPageNodeIds).Any() ||
    //        x.MicrositePages.Intersect(micrositePageNodeIds).Any();
    //}

    //private async Task<PersonCardDto[]> CreatePeopleCardDtosAsync(Person[] people, CancellationToken cancellationToken)
    //{
    //    var pageRelativeUrlByNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(people.Select(x => x.NodeID), _cultureContext.CurrentLanguageTag, cancellationToken);

    //    var dtos = await people.SelectOneByOneAsync(async person => new PersonCardDto
    //    {
    //        DisplayName = await person.GetPersonDisplayNameAsync(_settingsService, cancellationToken),
    //        TeaserImageUrl = person.MetadataTeaserImage?.ToHtmlCompatibleString(),
    //        TeaserImageAltText = person.MetadataTeaserImageAltText,
    //        JobTitle = person.PersonJobTitle,
    //        PersonUrl = pageRelativeUrlByNodeId[person.NodeID].ToHtmlCompatibleString(),
    //    });

    //    return dtos;
    //}
}
