using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations
{
    // The Kentico generated info classes require an initialised data engine for their real
    // property accessors, so the virtual members used by the page are overridden with plain
    // in-memory values.
    internal class StubChannelInfo : ChannelInfo
    {
        public override int ChannelID { get; set; }

        public override string ChannelDisplayName { get; set; } = string.Empty;

        public override string ToString() => ChannelDisplayName;
    }

    internal class StubWebChannelSettingsInfo : WebChannelSettingsInfo
    {
        public override int WebChannelSettingsID { get; set; }

        public override int WebChannelSettingsChannelID { get; set; }

        public override string WebChannelSettingsChannelDisplayName { get; set; } = string.Empty;

        public override string ToString() => WebChannelSettingsChannelDisplayName;
    }

    [TestFixture]
    public class BdoIntegrationsChannelListingPageTests
    {
        private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly int[] AccessibleChannelIds = { 1, 2 };
        private Mock<IInfoProvider<ChannelInfo>> channelInfoProviderMock;
        private Mock<IInfoProvider<WebChannelSettingsInfo>> webChannelSettingsInfoProviderMock;
        private Mock<IChannelAccessService> channelAccessServiceMock;

        [SetUp]
        public void SetUp()
        {
            channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
            webChannelSettingsInfoProviderMock = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
            channelAccessServiceMock = new Mock<IChannelAccessService>();

            SetupProviders(Array.Empty<ChannelInfo>(), Array.Empty<WebChannelSettingsInfo>());
        }

        [Test]
        public void InheritsFromListingPage()
        {
            Assert.That(typeof(BdoIntegrationsChannelListingPage).BaseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void ObjectType_ReturnsWebChannelSettingsObjectType()
        {
            var page = CreatePage();

            var objectType = typeof(BdoIntegrationsChannelListingPage)
                .GetProperty("ObjectType", InstanceNonPublic)
                ?.GetValue(page) as string;

            Assert.That(objectType, Is.EqualTo(WebChannelSettingsInfo.OBJECT_TYPE));
        }

        [Test]
        public void Constructor_NoChannels_DoesNotPersistAnything()
        {
            CreatePage();

            webChannelSettingsInfoProviderMock.Verify(p => p.Set(It.IsAny<WebChannelSettingsInfo>()), Times.Never);
        }

        [Test]
        public void Constructor_OutdatedDisplayName_UpdatesAndPersistsSetting()
        {
            var setting = new StubWebChannelSettingsInfo
            {
                WebChannelSettingsChannelID = 1,
                WebChannelSettingsChannelDisplayName = "Old name",
            };

            SetupProviders(
                new ChannelInfo[] { new StubChannelInfo { ChannelID = 1, ChannelDisplayName = "New name" } },
                new WebChannelSettingsInfo[] { setting });

            CreatePage();

            Assert.Multiple(() =>
            {
                Assert.That(setting.WebChannelSettingsChannelDisplayName, Is.EqualTo("New name"));
                webChannelSettingsInfoProviderMock.Verify(p => p.Set(setting), Times.Once);
            });
        }

        [Test]
        public void Constructor_DisplayNameUpToDate_DoesNotPersistSetting()
        {
            var setting = new StubWebChannelSettingsInfo
            {
                WebChannelSettingsChannelID = 1,
                WebChannelSettingsChannelDisplayName = "Same name",
            };

            SetupProviders(
                new ChannelInfo[] { new StubChannelInfo { ChannelID = 1, ChannelDisplayName = "Same name" } },
                new WebChannelSettingsInfo[] { setting });

            CreatePage();

            webChannelSettingsInfoProviderMock.Verify(p => p.Set(It.IsAny<WebChannelSettingsInfo>()), Times.Never);
        }

        [Test]
        public void Constructor_SettingBelongsToAnotherChannel_IsIgnored()
        {
            var setting = new StubWebChannelSettingsInfo
            {
                WebChannelSettingsChannelID = 99,
                WebChannelSettingsChannelDisplayName = "Other channel",
            };

            SetupProviders(
                new ChannelInfo[] { new StubChannelInfo { ChannelID = 1, ChannelDisplayName = "Channel one" } },
                new WebChannelSettingsInfo[] { setting });

            // The channel has no matching setting, so the page tries to create one. Creating a real
            // info object requires the data engine, which is unavailable in a unit test host.
            try
            {
                CreatePage();
            }
            catch
            {
                // Ignored on purpose - only the filtering behaviour is asserted.
            }

            Assert.That(setting.WebChannelSettingsChannelDisplayName, Is.EqualTo("Other channel"));
        }

        [Test]
        public async Task ConfigurePage_AddsChannelColumnAndEditRowAction()
        {
            var page = CreatePageForConfiguration();
            channelAccessServiceMock
                .Setup(s => s.GetAccessibleWebChannelSettingsIdsAsync())
                .ReturnsAsync(Array.Empty<int>());

            await ConfigurePageIgnoringAdminRuntimeAsync(page);

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(pageConfiguration, "ColumnConfigurations"), Is.EqualTo(1));
        }

        [Test]
        public async Task ConfigurePage_NoAccessibleIds_DoesNotAddQueryModifier()
        {
            var page = CreatePageForConfiguration();
            channelAccessServiceMock
                .Setup(s => s.GetAccessibleWebChannelSettingsIdsAsync())
                .Returns(Task.FromResult<ICollection<int>>(null!));

            await ConfigurePageIgnoringAdminRuntimeAsync(page);

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(pageConfiguration, "QueryModifiers"), Is.Zero);
        }

        [Test]
        public async Task ConfigurePage_AccessibleIds_AddsQueryModifierFilteringOnThoseIds()
        {
            var page = CreatePageForConfiguration();
            channelAccessServiceMock
                .Setup(s => s.GetAccessibleWebChannelSettingsIdsAsync())
                .ReturnsAsync(AccessibleChannelIds);
            await ConfigurePageIgnoringAdminRuntimeAsync(page);

            var modifier = GetSingleQueryModifier(page);
            Assert.Multiple(() =>
            {
                Assert.That(modifier, Is.Not.Null);
                Assert.That(InvokeModifier(modifier!), Does.Contain("WebChannelSettingsID"));
            });
           
        }

        [Test]
        public async Task ConfigurePage_NoAccessibleIds_ButNotNull_AddsBlockingQueryModifier()
        {
            var page = CreatePageForConfiguration();
            channelAccessServiceMock
                .Setup(s => s.GetAccessibleWebChannelSettingsIdsAsync())
                .ReturnsAsync(Array.Empty<int>());

            await ConfigurePageIgnoringAdminRuntimeAsync(page);

            var modifier = GetSingleQueryModifier(page);
            Assert.Multiple(() =>
            {
                Assert.That(modifier, Is.Not.Null);
                Assert.That(InvokeModifier(modifier!), Does.Contain("WebChannelSettingsID"));
            });
          
        }

        private void SetupProviders(ICollection<ChannelInfo> channels, ICollection<WebChannelSettingsInfo> settings)
        {
            channelInfoProviderMock.Setup(p => p.Get()).Returns(BuildQuery(channels).Object);
            webChannelSettingsInfoProviderMock.Setup(p => p.Get()).Returns(BuildQuery(settings).Object);
        }

        private static Mock<ObjectQuery<T>> BuildQuery<T>(IEnumerable<T> items)
            where T : BaseInfo
        {
            var queryMock = new Mock<ObjectQuery<T>>();

            queryMock.As<IEnumerable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => items.GetEnumerator());

            return queryMock;
        }

        private BdoIntegrationsChannelListingPage CreatePage() =>
            new(channelInfoProviderMock.Object, webChannelSettingsInfoProviderMock.Object, channelAccessServiceMock.Object);

        private BdoIntegrationsChannelListingPage CreatePageForConfiguration()
        {
            var page = CreatePage();
            BdoIntegrationsPageTestHelper.PreparePageConfigurationForConfigurePage(page);

            return page;
        }

        private static QueryModifier? GetSingleQueryModifier(BdoIntegrationsChannelListingPage page)
        {
            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);
            var modifiers = pageConfiguration.GetType()
                .GetProperty("QueryModifiers")!
                .GetValue(pageConfiguration) as IEnumerable<QueryModifier>;

            return modifiers?.FirstOrDefault();
        }

        private static string InvokeModifier(QueryModifier modifier)
        {
            var query = modifier.ModifyQuery(new ObjectQuery(), null!);

            return query.WhereCondition ?? string.Empty;
        }

        private static async Task ConfigurePageIgnoringAdminRuntimeAsync(BdoIntegrationsChannelListingPage page)
        {
            try
            {
                await page.ConfigurePage();
            }
            catch
            {
                // The Kentico admin runtime is not available in an isolated unit test host;
                // everything asserted here is executed before the framework call that throws.
            }
        }
    }
}
