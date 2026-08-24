namespace BDO.GWT.Core
{
    using BDO.GWT.Common.Core.Services;
    using BDO.GWT.Core;
    using BDO.GWT.Core.Helpers;
    using BDO.GWT.Core.LiveSite.Services;
    using BDO.GWT.Core.Retrievers;
    using BDO.GWT.Core.services;
    using BDO.GWT.Core.Services;
    using BDO.GWT.Core.Services.Footer;
    using BDO.GWT.Core.Services.Header;
    using BDO.GWT.Core.Services.Widget;
    using BDO.GWT.Core.ValueObjects;
    using BDO.GWT.Domain.Core.Helpers;
    using BDO.GWT.Domain.Core.Services;
    using BDO.GWT.GLOBAL.Services;
    using BDO.GWT.Web.Core.LiveSite.Services;
    using BDO.GWT.Web.Core.Services;
    using BDO.GWT.Web.Services;
    using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
    using BDO.GWT.Xperience.Core.LiveSite.Services;
    using BDO.GWT.Xperience.Core.Services;
    using CMS.ContentEngine;
    using CMS.Helpers;
    using CMS.Websites;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions

    {
        public static IServiceCollection AddContentServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IContentService, ContentService>();
            serviceCollection.AddScoped<IWebPageUrlResolver, WebPageUrlResolver>();
            serviceCollection.AddScoped<IPageUtility, PageUtility>();
            serviceCollection.AddScoped<IHomePageService, HomePageService>();
            serviceCollection.AddScoped<IHeroService, HeroService>();
            serviceCollection.AddScoped<IHeaderService, HeaderService>();
            serviceCollection.AddScoped<IWebsiteCulturesService, WebsiteCulturesService>();
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddScoped<ICultureContext, CultureContext>();
            serviceCollection.AddScoped<IHeaderService, HeaderService>();
            serviceCollection.AddScoped<IFooterService, FooterService>();
            serviceCollection.AddScoped<IReactService, ReactService>();
            serviceCollection.AddSingleton<IReactCachingService, ReactCachingService>();
            serviceCollection.AddScoped<ITileContentService, TileContentService>();
            serviceCollection.AddScoped<IFlexibleContentService, FlexibleContentService>();
            serviceCollection.AddScoped<IButtonUrlRetriever, ButtonUrlRetriever>();
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
            serviceCollection.AddScoped<IAuthoredContentService, AuthoredContentService>();
            serviceCollection.AddScoped<ISidebarService, SidebarService>();
            serviceCollection.AddScoped<ISocialMediaButtonsService, SocialMediaButtonsService>();
            serviceCollection.AddScoped<IExternalPeopleHelper, ExternalPeopleHelper>();
            serviceCollection.AddScoped<IFilteringHelper, FilteringHelper>();
            serviceCollection.AddScoped<ISettingService, SettingService>();
            serviceCollection.AddScoped<ISectionEventsService, SectionEventsService>();
            serviceCollection.AddScoped<IFormBuilderService, FormBuilderService>();
            serviceCollection.AddScoped<IIndustryServicePageService, IndustryServicePageService>();
            serviceCollection.AddScoped<ISubServicesPageService, SubServicesPageService>();
            serviceCollection.AddScoped<IServiceAreaAndServicePageService, ServiceAreaAndServicePageService>();
            serviceCollection.AddScoped<IBusinessLinePageService, BusinessLinePageService>();
            serviceCollection.AddScoped<IBlogContributorsPageService, BlogContributorsPageService>();
            serviceCollection.AddScoped<IBlogsPageService, BlogsPageService>();
            serviceCollection.AddScoped<IBlogPageCardsService, BlogPageCardsService>();
            serviceCollection.AddScoped<ISocialMediaButtonsService, SocialMediaButtonsService>();
            serviceCollection.AddScoped<ISpecialtiesPageService, SpecialtiesPageService>();
            serviceCollection.AddScoped<ISpecialtiesAreaPageService, SpecialtiesAreaPageService>();
            serviceCollection.AddScoped<ISpecialtiesCategoryPageService, SpecialtiesCategoryPageService>();
            serviceCollection.AddScoped<ISectionSpecialtiesService, SectionSpecialtiesService>();
            serviceCollection.AddScoped<IRecaptchaValidationService, RecaptchaValidationService>();
            serviceCollection.AddScoped<IRssSectionPageService, RssSectionPageService>();
            serviceCollection.AddScoped<IRssPageService, RssPageService>();
            serviceCollection.AddScoped<IBusinessStrategyService, BusinessStrategyService>();
            serviceCollection.AddScoped<ISectionBusinessStrategiesService, SectionBusinessStrategiesService>();

            //Widget Service
            serviceCollection.AddScoped<INewsCarouselService, NewsCarouselService>();
            serviceCollection.AddScoped<IKeyContactsService, KeyContactsService>();
            serviceCollection.AddScoped<ITabAccordionService, TabAccordionService>();
            serviceCollection.AddScoped<IEventsWebinarsSliderService, EventsWebinarsSliderService>();
            serviceCollection.AddScoped<ISitemapXmlRenderer, SitemapXmlRenderer>();
            serviceCollection.AddScoped<IEditorsPickCardService, EditorsPickCardService>();
            serviceCollection.AddScoped<IEditorsPickService, EditorsPickService>();
            serviceCollection.AddScoped<IRelatedContactsService, RelatedContactsService>();
            serviceCollection.AddScoped<IInFocusService, InFocusService>();
            serviceCollection.AddScoped<IInsightPageService, InsightPageService>();
            serviceCollection.AddScoped<IInsightCardService, InsightCardService>();
            serviceCollection.AddScoped<IInsightCarouselService, InsightCarouselService>();
            serviceCollection.AddScoped<IInsightCarouselStateStore, InsightCarouselStateStore>();
            serviceCollection.AddScoped<IEditorsPickStateStore, EditorsPickStateStore>();

            serviceCollection.AddScoped<IGlobalLocationCustomTableDataService, GlobalLocationCustomTableDataService>();
            serviceCollection.AddScoped<IEventWebinarPageService, EventWebinarPageService>();
            serviceCollection.AddScoped<ISectionIndustriesService, SectionIndustriesService>();
            serviceCollection.AddScoped<IIndustryCategoryPageService, IndustryCategoryPageService>();
            serviceCollection.AddScoped<IIndustryPageService, IndustryPageService>();
            serviceCollection.AddScoped<IIndustryCategoryPageService, IndustryCategoryPageService>();
            serviceCollection.AddScoped<ISectionServicesService, SectionServicesService>();
            serviceCollection.AddScoped<ICareerPageService, CareerPageService>();
            serviceCollection.AddScoped<ICareersPageService, CareersPageService>();
            serviceCollection.AddScoped<ICareerDetailPageService, CareerDetailPageService>();
            serviceCollection.AddScoped<ICareerCategoryPageService, CareerCategoryPageService>();
            serviceCollection.AddScoped<ISectionBlogService, SectionBlogService>();
            serviceCollection.AddScoped<IBlogPostPageService, BlogPostPageService>();
            serviceCollection.AddScoped<ILegalPageService, LegalPageService>();
            serviceCollection.AddScoped<IDealPageService, DealPageService>();
            serviceCollection.AddScoped<ISectionDealsPageService, SectionDealsPageService>();
            serviceCollection.AddScoped<IInsightSectionCardListingPageService, InsightSectionCardListingPageService>();
            serviceCollection.AddScoped<IInsightGroupCardListingPageService, InsightGroupCardListingPageService>();
            serviceCollection.AddScoped<IInsightCategoryCardListingPageService, InsightCategoryCardListingPageService>();
            serviceCollection.AddScoped<IHeroService, HeroService>();
            serviceCollection.AddScoped<ISectionAboutPageService, SectionAboutPageService>();
            serviceCollection.AddScoped<IAboutPageService, AboutPageService>();
            serviceCollection.AddScoped<ICareerContentPageService, CareerContentPageService>();
            serviceCollection.AddScoped<IEventWebinarCardsService, EventWebinarCardsService>();
            serviceCollection.AddScoped<IEventRegistrationFormPageService, EventRegistrationFormPageService>();
            serviceCollection.AddScoped<IDealCardsService, DealCardsService>();
            serviceCollection.AddScoped<ITimeZoneService, TimeZoneService>();
            serviceCollection.AddScoped<ICookieBannerService, CookieBannerService>();

            serviceCollection.AddScoped<ISitemapPageService, SitemapPageService>();
            serviceCollection.AddScoped<ISitemapService, SitemapService>();
            serviceCollection.AddScoped<ILanguageProvider, LanguageProvider>();
            serviceCollection.AddScoped<IPageNodeProvider, PageNodeProvider>();
            serviceCollection.AddScoped<ISubscribePageService, SubscribePageService>();
            serviceCollection.AddScoped<IContactPageService, ContactPageService>();
            serviceCollection.AddScoped<ISubscribeFormService, SubscribeFormService>();
            serviceCollection.AddScoped<ISubscribeConfirmPageService, SubscribeConfirmPageService>();
            serviceCollection.AddScoped<ISubscribeRemovePageService, SubscribeRemovePageService>();
            serviceCollection.AddScoped<INewsletterSubscriptionService, NewsletterSubscriptionService>();
            serviceCollection.AddScoped<IMarketingContactManagementService, MarketingContactManagementService>();
            serviceCollection.AddScoped<ICookieConsentService, CookieConsentService>();
            serviceCollection.AddScoped<IPageUrlService, PageUrlService>();
            serviceCollection.AddScoped<ISiteContext, SiteContext>();
            serviceCollection.AddScoped<IVisitorContext, VisitorContext>();
            serviceCollection.AddScoped<IHashSaltProvider, HashSaltProvider>();
            serviceCollection.AddScoped<ILocalizedUrlsBuilder, LocalizedUrlsBuilder>();
            serviceCollection.AddScoped<ICultureService, CultureService>();
            serviceCollection.AddScoped<IChannelAccessService, ChannelAccessService>();
            serviceCollection.AddScoped<IDateTimeProvider, DateTimeProvider>();
            serviceCollection.AddScoped<IRelatedContentCardService, RelatedContentCardService>();
            serviceCollection.AddScoped<IRelatedContentCarouselService, RelatedContentCarouselService>();
            serviceCollection.AddScoped<IFormSubmissionSubscriptionsService, FormSubmissionSubscriptionsService>();

            return serviceCollection;
        }
    }
}

