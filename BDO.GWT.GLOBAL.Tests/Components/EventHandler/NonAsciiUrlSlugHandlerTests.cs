using BDO.GWT.GLOBAL.Components.EventHandler;
using BDO.GWT.GLOBAL.Services;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.Tests.Components.EventHandler;

public class NonAsciiUrlSlugHandlerTests
{
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock = null!;
    private Mock<IServiceScope> _serviceScopeMock = null!;
    private Mock<IServiceProvider> _scopedServiceProviderMock = null!;
    private Mock<INonAsciiUrlReplacementService> _replacementServiceMock = null!;
    private NonAsciiUrlSlugHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _replacementServiceMock = new Mock<INonAsciiUrlReplacementService>(MockBehavior.Strict);
        _scopedServiceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        _serviceScopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);

        // Setup the scope factory to return our mock scope
        _serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        // Setup the scope to return our mock service provider
        _serviceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(_scopedServiceProviderMock.Object);

        // Setup the scoped service provider to return our mock replacement service
        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(INonAsciiUrlReplacementService)))
            .Returns(_replacementServiceMock.Object);

        // Setup disposal
        _serviceScopeMock
            .Setup(x => x.Dispose());

        _handler = new NonAsciiUrlSlugHandler(_serviceScopeFactoryMock.Object, NullLogger<NonAsciiUrlSlugHandler>.Instance);
    }

    [Test]
    public async Task HandleAsync_AppliesReplacementWhenSlugIsNotProvided()
    {
        _replacementServiceMock
            .Setup(x => x.ApplyReplacementsAsync("btpnewä", 123, It.IsAny<CancellationToken>()))
            .ReturnsAsync("btpnewae");

        var data = new CreateWebPageEventData
        {
            DisplayName = "btpnewä",
            WebsiteChannelID = 123,
            WebsiteChannelName = "Main",
            ContentTypeName = "BDO.Page",
            ContentLanguageName = "en-US"
        };

        var asyncEvent = CreateBeforeCreateWebPageEvent(data);

        await _handler.HandleAsync(asyncEvent, CancellationToken.None);

        Assert.That(data.UrlPathSlug, Is.EqualTo("btpnewae"));
        _replacementServiceMock.VerifyAll();
    }

    [Test]
    public async Task HandleAsync_OverwritesUrlSlugWithCustomReplacement()
    {
        // New behavior: Always apply custom replacement, even if UrlPathSlug was pre-filled
        // This ensures Kentico's default normalization is replaced with ours
        _replacementServiceMock
            .Setup(x => x.ApplyReplacementsAsync("btpnewä", 123, It.IsAny<CancellationToken>()))
            .ReturnsAsync("btpnewae");

        var data = new CreateWebPageEventData
        {
            DisplayName = "btpnewä",
            UrlPathSlug = "manual-slug", // Even if user provided one, we override it
            WebsiteChannelID = 123,
            WebsiteChannelName = "Main",
            ContentTypeName = "BDO.Page",
            ContentLanguageName = "en-US"
        };

        var asyncEvent = CreateBeforeCreateWebPageEvent(data);

        await _handler.HandleAsync(asyncEvent, CancellationToken.None);

        // The handler now replaces the slug with the normalized version
        Assert.That(data.UrlPathSlug, Is.EqualTo("btpnewae"));
        _replacementServiceMock.VerifyAll();
    }

    private static BeforeCreateWebPageEvent CreateBeforeCreateWebPageEvent(CreateWebPageEventData data)
    {
        var instance = (BeforeCreateWebPageEvent)RuntimeHelpers.GetUninitializedObject(typeof(BeforeCreateWebPageEvent));
        SetPropertyOrBackingField(instance, nameof(BeforeCreateWebPageEvent.Data), data);
        return instance;
    }

    private static void SetPropertyOrBackingField(object instance, string propertyName, object? value)
    {
        var backingFieldName = $"<{propertyName}>k__BackingField";
        for (var t = instance.GetType(); t != null && t != typeof(object); t = t.BaseType)
        {
            var field = t.GetField(backingFieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly)
                ?? t.GetField(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);
            if (field != null)
            {
                field.SetValue(instance, value);
                return;
            }
        }

        throw new InvalidOperationException($"Could not set '{propertyName}' on '{instance.GetType().FullName}'.");
    }
}
