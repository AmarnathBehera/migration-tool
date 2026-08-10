using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class SectionEventsCrudServiceTests
{
    private static SectionEventsCrudService CreateServiceWithoutConstructor() =>
        (SectionEventsCrudService)RuntimeHelpers.GetUninitializedObject(typeof(SectionEventsCrudService));

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageAsync(
            channelId: 1,
            WebPageParentID: 10,
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.CreatePageCultureVersionAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "en-us",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequest()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.UpdatePageAsync(
            channelId: 1,
            WebPageItemGuid: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public void SectionEventsCrudService_ImplementsISectionEventsCrudService()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.That(service, Is.InstanceOf<ISectionEventsCrudService>());
    }

    [Test]
    public void SectionEventsPageContent_DefaultValues_AreInitialized()
    {
        var content = new SectionEventsPageContent();

        Assert.Multiple(() =>
        {
            Assert.That(content, Is.Not.Null);
            Assert.That(content.EventsWebinarsListFilters, Is.Empty);
            Assert.That(content.EventsWebinarsListFeaturedPages, Is.Empty);
        });
    }

    [Test]
    public void SectionEventsPageContent_StringProperties_CanBeSet()
    {
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "Events Title",
            MetadataTeaserImageAltText = "Alt",
            HeroLayout = "default",
            MetadataDescription = "Desc",
            MetadataTeaserImage = "img.jpg",
            MetadataCanonical = "https://example.com",
            HeroTitle = "Hero Title",
            HeroDescription = "Hero Desc",
            HeroBackgroundImage = "hero.jpg",
            HeroBackgroundImageAltText = "hero alt",
            HeroBackgroundImage_Mobile = "hero-m.jpg",
            HeroBackgroundImage_MobileAltText = "hero mobile alt",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "Click me",
            HeroCallToActionButtonUrl = "https://cta.com",
            EventsWebinarsListTitle = "List Title",
            EventsWebinarsListDescription = "List Desc",
            EventsWebinarsListAdvancedSearchDescription = "Search Desc",
            EventsWebinarsListAdvancedSearchLinkText = "Search Link",
            EventsWebinarsListAdvancedSearchLinkUrl = "https://search.com",
            MetadataOGTitle = "OG Title",
            MetadataOGDescription = "OG Desc",
            MetadataOGImage = "og.jpg",
            MetadataOGTwitterImage = "tw.jpg",
            RedirectOnUnpublish = "/redirect"
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.MetadataTitle, Is.EqualTo("Events Title"));
            Assert.That(content.MetadataTeaserImageAltText, Is.EqualTo("Alt"));
            Assert.That(content.HeroLayout, Is.EqualTo("default"));
            Assert.That(content.MetadataDescription, Is.EqualTo("Desc"));
            Assert.That(content.MetadataTeaserImage, Is.EqualTo("img.jpg"));
            Assert.That(content.MetadataCanonical, Is.EqualTo("https://example.com"));
            Assert.That(content.HeroTitle, Is.EqualTo("Hero Title"));
            Assert.That(content.HeroDescription, Is.EqualTo("Hero Desc"));
            Assert.That(content.HeroBackgroundImage, Is.EqualTo("hero.jpg"));
            Assert.That(content.HeroBackgroundImageAltText, Is.EqualTo("hero alt"));
            Assert.That(content.HeroBackgroundImage_Mobile, Is.EqualTo("hero-m.jpg"));
            Assert.That(content.HeroBackgroundImage_MobileAltText, Is.EqualTo("hero mobile alt"));
            Assert.That(content.HeroCallToActionButtonType, Is.EqualTo("primary"));
            Assert.That(content.HeroCallToActionButtonText, Is.EqualTo("Click me"));
            Assert.That(content.HeroCallToActionButtonUrl, Is.EqualTo("https://cta.com"));
            Assert.That(content.EventsWebinarsListTitle, Is.EqualTo("List Title"));
            Assert.That(content.EventsWebinarsListDescription, Is.EqualTo("List Desc"));
            Assert.That(content.EventsWebinarsListAdvancedSearchDescription, Is.EqualTo("Search Desc"));
            Assert.That(content.EventsWebinarsListAdvancedSearchLinkText, Is.EqualTo("Search Link"));
            Assert.That(content.EventsWebinarsListAdvancedSearchLinkUrl, Is.EqualTo("https://search.com"));
            Assert.That(content.MetadataOGTitle, Is.EqualTo("OG Title"));
            Assert.That(content.MetadataOGDescription, Is.EqualTo("OG Desc"));
            Assert.That(content.MetadataOGImage, Is.EqualTo("og.jpg"));
            Assert.That(content.MetadataOGTwitterImage, Is.EqualTo("tw.jpg"));
            Assert.That(content.RedirectOnUnpublish, Is.EqualTo("/redirect"));
        });
    }

    [Test]
    public void SectionEventsPageContent_BoolProperties_CanBeSet()
    {
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            HeroLayout = "L",
            EventsWebinarsListFilteringEnabled = true,
            ShowPastEvents = true,
            MetadataOGTwitterImageSizeCheckbox = true,
            MetadataNofollow = true,
            MetadataNoindex = true,
            IncludeInSitemap = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.EventsWebinarsListFilteringEnabled, Is.True);
            Assert.That(content.ShowPastEvents, Is.True);
            Assert.That(content.MetadataOGTwitterImageSizeCheckbox, Is.True);
            Assert.That(content.MetadataNofollow, Is.True);
            Assert.That(content.MetadataNoindex, Is.True);
            Assert.That(content.IncludeInSitemap, Is.True);
        });
    }

    [Test]
    public void SectionEventsPageContent_CollectionProperties_CanBePopulated()
    {
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            HeroLayout = "L",
            EventsWebinarsListFilters = ["filter1", "filter2"],
            EventsWebinarsListFeaturedPages = ["page1"]
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.EventsWebinarsListFilters, Has.Count.EqualTo(2));
            Assert.That(content.EventsWebinarsListFeaturedPages, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void SectionEventsPageContent_DateProperties_CanBeSet()
    {
        var from = new DateTime(2024, 1, 1);
        var to = new DateTime(2024, 12, 31);
        var content = new SectionEventsPageContent
        {
            MetadataTitle = "T",
            MetadataTeaserImageAltText = "A",
            HeroLayout = "L",
            PublishFrom = from,
            PublishTo = to
        };

        Assert.Multiple(() =>
        {
            Assert.That(content.PublishFrom, Is.EqualTo(from));
            Assert.That(content.PublishTo, Is.EqualTo(to));
        });
    }

    [Test]
    public void PageOutput_SectionEventsPageContent_CanBeConstructed()
    {
        var guid = Guid.NewGuid();
        var content = new SectionEventsPageContent();
        var output = new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput { PageGuid = guid },
            Content = content
        };

        Assert.Multiple(() =>
        {
            Assert.That(output.General.PageGuid, Is.EqualTo(guid));
            Assert.That(output.Content, Is.SameAs(content));
        });
    }

    [Test]
    public async Task GetPagesAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, "en-gb", true, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetPagesAsync_WithNullCulture_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPagesAsync(1, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public async Task GetPageAsync_WhenDependenciesUnavailable_ReturnsInternalServerError()
    {
        var service = CreateServiceWithoutConstructor();

        var result = await service.GetPageAsync(1, Guid.NewGuid(), "en-gb", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result.Result!).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        });
    }

    [Test]
    public void DeletePageAsync_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", false, CancellationToken.None));
    }

    [Test]
    public void DeletePageAsync_WithDeleteAllCultures_WhenDependenciesUnavailable_Throws()
    {
        var service = CreateServiceWithoutConstructor();

        Assert.ThrowsAsync<NullReferenceException>(async () =>
            await service.DeletePageAsync(1, Guid.NewGuid(), "en-gb", true, CancellationToken.None));
    }

    [Test]
    public void SectionEventsCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(ISectionEventsCrudService);

        Assert.Multiple(() =>
        {
            Assert.That(interfaceType.GetMethod("GetPagesAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("GetPageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("CreatePageCultureVersionAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("UpdatePageAsync"), Is.Not.Null);
            Assert.That(interfaceType.GetMethod("DeletePageAsync"), Is.Not.Null);
        });
    }
}
