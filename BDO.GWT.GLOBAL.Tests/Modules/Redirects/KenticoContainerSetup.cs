using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;
using CMS.Websites;
using CMS.Websites.Internal;
using Moq;
using CoreService = global::CMS.Core.Service;
using RedirectInfoX = global::XperienceCommunity.Redirects.RedirectInfo;

namespace BDO.GWT.GLOBAL.Tests;

/// <summary>
/// Assembly-wide setup that boots a minimal Kentico service container populated with
/// mock <see cref="IInfoProvider{TInfo}"/> instances. This lets tests exercise production
/// code that reaches Kentico's static <c>XxxInfo.Provider</c> accessors, which resolve from
/// this container. NUnit runs tests sequentially (no <c>[Parallelizable]</c>), so tests can
/// safely reconfigure the shared mocks between runs via <see cref="Reset"/>.
/// </summary>
[SetUpFixture]
public class KenticoContainerSetup
{
    public static Mock<IInfoProvider<RedirectInfoX>> RedirectProvider { get; private set; } = null!;
    public static Mock<IInfoProvider<WebPageItemInfo>> WebPageItemProvider { get; private set; } = null!;
    public static Mock<IInfoProvider<ContentLanguageInfo>> ContentLanguageProvider { get; private set; } = null!;
    public static Mock<IInfoProvider<WebsiteChannelInfo>> WebsiteChannelProvider { get; private set; } = null!;
    public static Mock<IInfoProvider<ChannelInfo>> ChannelProvider { get; private set; } = null!;
    public static Mock<IInfoProvider<ResourceInfo>> ResourceProvider { get; private set; } = null!;

    [OneTimeSetUp]
    public void InitContainer()
    {
        RedirectProvider = new Mock<IInfoProvider<RedirectInfoX>>();
        WebPageItemProvider = new Mock<IInfoProvider<WebPageItemInfo>>();
        ContentLanguageProvider = new Mock<IInfoProvider<ContentLanguageInfo>>();
        WebsiteChannelProvider = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        ChannelProvider = new Mock<IInfoProvider<ChannelInfo>>();
        ResourceProvider = new Mock<IInfoProvider<ResourceInfo>>();

        // Pre-add the "by id"/"by name" provider interfaces BEFORE the mock's Object is created
        // (Moq requires additional interfaces be declared before first Object access). The static
        // XxxInfo.Provider.Get(int)/Get(string) extension methods cast the provider to these.
        RedirectProvider.As<IInfoByIdProvider<RedirectInfoX>>();
        WebsiteChannelProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>();
        ChannelProvider.As<IInfoByIdProvider<ChannelInfo>>();
        ResourceProvider.As<IInfoByNameProvider<ResourceInfo>>();

        CoreService.Use<IInfoProvider<RedirectInfoX>>(RedirectProvider.Object);
        CoreService.Use<IInfoProvider<WebPageItemInfo>>(WebPageItemProvider.Object);
        CoreService.Use<IInfoProvider<ContentLanguageInfo>>(ContentLanguageProvider.Object);
        CoreService.Use<IInfoProvider<WebsiteChannelInfo>>(WebsiteChannelProvider.Object);
        CoreService.Use<IInfoProvider<ChannelInfo>>(ChannelProvider.Object);
        CoreService.Use<IInfoProvider<ResourceInfo>>(ResourceProvider.Object);

        // Core infrastructure services that Kentico base classes resolve during construction.
        CoreService.Use<ILocalizationService>(new Mock<ILocalizationService>().Object);
        CoreService.Use<IConversionService>(new Mock<IConversionService>().Object);
        CoreService.Use<IEventLogService>(new Mock<IEventLogService>().Object);
        CoreService.Use<ISettingsService>(new Mock<ISettingsService>().Object);

        CoreService.InitializeContainer();
    }

    /// <summary>Resets all shared provider mocks so a test starts from a clean slate.</summary>
    public static void Reset()
    {
        RedirectProvider.Reset();
        WebPageItemProvider.Reset();
        ContentLanguageProvider.Reset();
        WebsiteChannelProvider.Reset();
        ChannelProvider.Reset();
        ResourceProvider.Reset();
    }
}
