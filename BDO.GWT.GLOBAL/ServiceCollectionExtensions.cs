using BDO.GWT.Common.Options;
using BDO.GWT.Core.Entities;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.DTOs.Cards;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using BDO.GWT.Xperience.Core.Services;
using CMS.Core;
using GWTXbyKWeb.Components.Services.SEOOptions;
using Microsoft.Extensions.DependencyInjection;

namespace BDO.GWT.Common;

public static class ServiceCollectionExtensions
{
    public static void AddXperience(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<CachingOptions>()
           .BindConfiguration("Caching")
           .ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactCachingOptions>()
            .BindConfiguration("Caching:React")
            .ValidateDataAnnotations();

        serviceCollection.AddOptions<ReactOptions>()
          .BindConfiguration("React")
          .ValidateDataAnnotations();

        serviceCollection.AddHttpClient();

        serviceCollection.AddScoped<GlobalSettingsService>();
        serviceCollection.AddScoped<IContactPageService, ContactPageService>();
        serviceCollection.AddScoped<IHeroService, HeroService>();
        serviceCollection.AddScoped<XperiencePageContext>();
        serviceCollection.AddScoped<IWebsiteContext, WebsiteContext>();
        serviceCollection.AddScoped<IHeaderService, HeaderService>();
        serviceCollection.AddScoped<IFooterService, FooterService>();
        serviceCollection.AddScoped<ICookieBannerService, CookieBannerService>();
        serviceCollection.ConfigureOptions<SEOOptionsSetup>();
        serviceCollection.AddScoped<IReactService, ReactService>();
        serviceCollection.AddSingleton<IReactCachingService, ReactCachingService>();
        serviceCollection.AddScoped<IPageContext, XperiencePageContext>();
        serviceCollection.AddScoped<IPageBuilderSectionService, PageBuilderSectionService>();
        serviceCollection.AddScoped<ITileContentService, TileContentService>();
        serviceCollection.AddScoped<IButtonUrlRetriever, ButtonUrlRetriever>();
        serviceCollection.AddScoped<IContentRetrieverWrapper, ContentRetrieverWrapper>();
        serviceCollection.AddScoped<IHeroService, HeroService>();
        serviceCollection.AddScoped<ISectionAboutPageService, SectionAboutPageService>();
        serviceCollection.AddScoped<IAboutPageService, AboutPageService>();
        serviceCollection.AddScoped<ISectionPeoplePageService, SectionPeoplePageService>();
        serviceCollection.AddScoped<ICardsService, CardsService>();
        serviceCollection.AddScoped<IPeopleCardsService, PeopleCardsService>();
        serviceCollection.AddScoped<IPersonPageService, PersonPageService>();
        serviceCollection.AddScoped<IPersonHelper, PersonHelper>();
        serviceCollection.AddScoped<ILocationCityPageService, LocationCityPageService>();
        serviceCollection.AddScoped<ILocationsPageService, LocationsPageService>();
        serviceCollection.AddScoped<ITaxonomyTagRetriever, TaxonomyTagRetriever>();
        serviceCollection.AddScoped<IFaqService, FaqService>();
        serviceCollection.AddScoped<INewsSectionPageService, NewsSectionPageService>();
        serviceCollection.AddScoped<INewsGroupPageService, NewsGroupPageService>();
        serviceCollection.AddScoped<INewsArticleService, NewsArticleService>();
        serviceCollection.AddScoped<INewsArticleCardsService, NewsArticleCardsService>();
        serviceCollection.AddScoped<ISidebarService, SidebarService>();
        serviceCollection.AddScoped<ISocialMediaButtonsService, SocialMediaButtonsService>();
        serviceCollection.AddScoped<IExternalPeopleHelper, ExternalPeopleHelper>();
        serviceCollection.AddScoped<IFilteringHelper, FilteringHelper>();
        serviceCollection.AddScoped<ISettingService, SettingService>();
        serviceCollection.AddScoped<ITimeZoneService, TimeZoneService>();
        serviceCollection.AddScoped<ISectionEventsService, SectionEventsService>();
        serviceCollection.AddScoped<IEventWebinarCardsService, EventWebinarCardsService>(); // ADD THIS
        serviceCollection.AddScoped<IEventRegistrationFormPageService, EventRegistrationFormPageService>();
        serviceCollection.AddScoped<IEventWebinarPageService, EventWebinarPageService>();

        serviceCollection.AddScoped<IBreadcrumbsService, BreadcrumbsService>();
        serviceCollection.AddScoped<IFormBuilderService, FormBuilderService>();
        serviceCollection.AddScoped<ISectionIndustriesService, SectionIndustriesService>();
        serviceCollection.AddScoped<IIndustryCategoryPageService, IndustryCategoryPageService>();
        serviceCollection.AddScoped<IIndustryPageService, IndustryPageService>();
        serviceCollection.AddScoped<IIndustryServicePageService, IndustryServicePageService>();
        serviceCollection.AddScoped<IIndustryCategoryPageService, IndustryCategoryPageService>();
        serviceCollection.AddScoped<ISectionServicesService, SectionServicesService>();
        serviceCollection.AddScoped<ISubServicesPageService, SubServicesPageService>();
        serviceCollection.AddScoped<IServiceAreaAndServicePageService, ServiceAreaAndServicePageService>();
        serviceCollection.AddScoped<IBusinessLinePageService, BusinessLinePageService>();
        serviceCollection.AddTransient<DropdownScriptExecutor>();
        serviceCollection.AddScoped<IChannelAccessService, ChannelAccessService>();
    }

}
