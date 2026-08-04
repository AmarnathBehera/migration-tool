using BDO.GWT.Common;
using BDO.GWT.Common.Options;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;
using BDO.GWT.GLOBAL.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Services;
using BDO.GWT.Xperience;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using BDO.GWT.Xperience.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BDO.GWT.GLOBAL.Tests;

public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        // Add a minimal configuration so BindConfiguration calls don't fail
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:Enabled"] = "true",
                ["Caching:React:Enabled"] = "true",
                ["React:BaseUrl"] = "http://localhost"
            })
            .Build();

        _services.AddSingleton<IConfiguration>(configuration);
    }

    [Test]
    public void AddXperience_RegistersOptionsConfigurations()
    {
        _services.AddXperience();

        var provider = _services.BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(_services.Any(sd => sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(CachingOptions)),
                Is.True, "CachingOptions should be configured");

            Assert.That(_services.Any(sd => sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(ReactCachingOptions)),
                Is.True, "ReactCachingOptions should be configured");

            Assert.That(_services.Any(sd => sd.ServiceType.IsGenericType &&
                sd.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(ReactOptions)),
                Is.True, "ReactOptions should be configured");
        });
    }

    [Test]
    public void AddXperience_RegistersHttpClient()
    {
        _services.AddXperience();

        Assert.That(_services.Any(sd => sd.ServiceType == typeof(IHttpClientFactory)), Is.True);
    }

    [Test]
    [TestCase(typeof(IContactPageService), typeof(ContactPageService))]
    [TestCase(typeof(IHeroService), typeof(HeroService))]
    [TestCase(typeof(IWebsiteContext), typeof(WebsiteContext))]
    [TestCase(typeof(IHeaderService), typeof(HeaderService))]
    [TestCase(typeof(IMicrositeHeaderService), typeof(MicrositeHeaderService))]
    [TestCase(typeof(IFooterService), typeof(FooterService))]
    [TestCase(typeof(ICookieBannerService), typeof(CookieBannerService))]
    [TestCase(typeof(IReactService), typeof(ReactService))]
    [TestCase(typeof(IPageContext), typeof(XperiencePageContext))]
    [TestCase(typeof(IPageBuilderSectionService), typeof(PageBuilderSectionService))]
    [TestCase(typeof(ITileContentService), typeof(TileContentService))]
    [TestCase(typeof(IFlexibleContentService), typeof(FlexibleContentService))]
    [TestCase(typeof(IButtonUrlRetriever), typeof(ButtonUrlRetriever))]
    [TestCase(typeof(ISectionAboutPageService), typeof(SectionAboutPageService))]
    [TestCase(typeof(IAboutPageService), typeof(AboutPageService))]
    [TestCase(typeof(ISectionPeoplePageService), typeof(SectionPeoplePageService))]
    [TestCase(typeof(ICardsService), typeof(CardsService))]
    [TestCase(typeof(IPeopleCardsService), typeof(PeopleCardsService))]
    [TestCase(typeof(IPersonPageService), typeof(PersonPageService))]
    [TestCase(typeof(IPersonHelper), typeof(PersonHelper))]
    [TestCase(typeof(ILocationCityPageService), typeof(LocationCityPageService))]
    [TestCase(typeof(ILocationsPageService), typeof(LocationsPageService))]
    [TestCase(typeof(ITaxonomyTagRetriever), typeof(TaxonomyTagRetriever))]
    [TestCase(typeof(IFaqService), typeof(FaqService))]
    [TestCase(typeof(INewsSectionPageService), typeof(NewsSectionPageService))]
    [TestCase(typeof(INewsGroupPageService), typeof(NewsGroupPageService))]
    [TestCase(typeof(INewsArticleService), typeof(NewsArticleService))]
    [TestCase(typeof(INewsArticleCardsService), typeof(NewsArticleCardsService))]
    [TestCase(typeof(IAuthoredContentService), typeof(AuthoredContentService))]
    [TestCase(typeof(ISidebarService), typeof(SidebarService))]
    [TestCase(typeof(ISocialMediaButtonsService), typeof(SocialMediaButtonsService))]
    [TestCase(typeof(IExternalPeopleHelper), typeof(ExternalPeopleHelper))]
    [TestCase(typeof(IFilteringHelper), typeof(FilteringHelper))]
    [TestCase(typeof(ISettingService), typeof(SettingService))]
    [TestCase(typeof(ITimeZoneService), typeof(TimeZoneService))]
    [TestCase(typeof(ISectionEventsService), typeof(SectionEventsService))]
    [TestCase(typeof(IEventWebinarCardsService), typeof(EventWebinarCardsService))]
    [TestCase(typeof(IEventRegistrationFormPageService), typeof(EventRegistrationFormPageService))]
    [TestCase(typeof(IEventWebinarPageService), typeof(EventWebinarPageService))]
    [TestCase(typeof(IBreadcrumbsService), typeof(BreadcrumbsService))]
    [TestCase(typeof(IFormBuilderService), typeof(FormBuilderService))]
    [TestCase(typeof(ISectionIndustriesService), typeof(SectionIndustriesService))]
    [TestCase(typeof(IIndustryCategoryPageService), typeof(IndustryCategoryPageService))]
    [TestCase(typeof(IIndustryPageService), typeof(IndustryPageService))]
    [TestCase(typeof(IIndustryServicePageService), typeof(IndustryServicePageService))]
    [TestCase(typeof(ISectionServicesService), typeof(SectionServicesService))]
    [TestCase(typeof(ISubServicesPageService), typeof(SubServicesPageService))]
    [TestCase(typeof(IServiceAreaAndServicePageService), typeof(ServiceAreaAndServicePageService))]
    [TestCase(typeof(IBusinessLinePageService), typeof(BusinessLinePageService))]
    [TestCase(typeof(ICareerPageService), typeof(CareerPageService))]
    [TestCase(typeof(ICareersPageService), typeof(CareersPageService))]
    [TestCase(typeof(ICareerDetailPageService), typeof(CareerDetailPageService))]
    [TestCase(typeof(ICareerCategoryPageService), typeof(CareerCategoryPageService))]
    [TestCase(typeof(ICareerContentPageService), typeof(CareerContentPageService))]
    [TestCase(typeof(IBlogContributorsPageService), typeof(BlogContributorsPageService))]
    [TestCase(typeof(IChannelAccessService), typeof(ChannelAccessService))]
    [TestCase(typeof(ISectionBlogService), typeof(SectionBlogService))]
    [TestCase(typeof(IBlogPostPageService), typeof(BlogPostPageService))]
    [TestCase(typeof(IBlogsPageService), typeof(BlogsPageService))]
    [TestCase(typeof(IBlogPageCardsService), typeof(BlogPageCardsService))]
    [TestCase(typeof(ISpecialtiesPageService), typeof(SpecialtiesPageService))]
    [TestCase(typeof(ISpecialtiesAreaPageService), typeof(SpecialtiesAreaPageService))]
    [TestCase(typeof(ISpecialtiesCategoryPageService), typeof(SpecialtiesCategoryPageService))]
    [TestCase(typeof(ISectionSpecialtiesService), typeof(SectionSpecialtiesService))]
    [TestCase(typeof(IMicrositeMasterPageService), typeof(MicrositeMasterPageService))]
    [TestCase(typeof(IMicrositePageService), typeof(MicrositePageService))]
    [TestCase(typeof(IDealPageService), typeof(DealPageService))]
    [TestCase(typeof(IRecaptchaValidationService), typeof(RecaptchaValidationService))]
    [TestCase(typeof(IRssSectionPageService), typeof(RssSectionPageService))]
    [TestCase(typeof(IRssPageService), typeof(RssPageService))]
    [TestCase(typeof(ISectionDealsPageService), typeof(SectionDealsPageService))]
    [TestCase(typeof(INewsCarouselService), typeof(NewsCarouselService))]
    [TestCase(typeof(ITabAccordionService), typeof(TabAccordionService))]
    [TestCase(typeof(IEventsWebinarsSliderService), typeof(EventsWebinarsSliderService))]
    [TestCase(typeof(ISitemapPageService), typeof(SitemapPageService))]
    [TestCase(typeof(ISitemapService), typeof(SitemapService))]
    [TestCase(typeof(ISitemapXmlRenderer), typeof(SitemapXmlRenderer))]
    [TestCase(typeof(ILanguageProvider), typeof(LanguageProvider))]
    [TestCase(typeof(IPageNodeProvider), typeof(PageNodeProvider))]
    [TestCase(typeof(IGDPRUserLookupService), typeof(GDPRUserService))]
    public void AddXperience_RegistersScopedService(Type serviceType, Type implementationType)
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd => sd.ServiceType == serviceType);

        Assert.That(descriptor, Is.Not.Null, $"{serviceType.Name} should be registered");
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(implementationType),
                $"{serviceType.Name} should map to {implementationType.Name}");
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped),
                $"{serviceType.Name} should be registered as Scoped");
        });
    }

    [Test]
    public void AddXperience_RegistersReactCachingServiceAsSingleton()
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd => sd.ServiceType == typeof(IReactCachingService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(ReactCachingService)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        });
    }

    [Test]
    public void AddXperience_RegistersDropdownScriptExecutorAsTransient()
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd => sd.ServiceType == typeof(DropdownScriptExecutor));

        Assert.That(descriptor, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(DropdownScriptExecutor)));
            Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Transient));
        });
    }

    [Test]
    public void AddXperience_RegistersXperiencePageContextDirectly()
    {
        _services.AddXperience();

        var descriptor = _services.FirstOrDefault(sd =>
            sd.ServiceType == typeof(XperiencePageContext) &&
            sd.ImplementationType == typeof(XperiencePageContext));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void AddXperience_CanBeCalledWithoutThrowingException()
    {
        Assert.DoesNotThrow(() => _services.AddXperience());
    }
}