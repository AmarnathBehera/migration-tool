using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.DataEngine;
using Microsoft.Extensions.Localization;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingApplicationChannelSettingsListingPageTests
    {
        private Mock<IInfoProvider<CMS.ContentEngine.ChannelInfo>> channelProviderMock = null!;
        private Mock<IInfoProvider<ReportingChannelSettingInfo>> settingProviderMock = null!;
        private Mock<IStringLocalizer<SharedResources>> localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            channelProviderMock = new Mock<IInfoProvider<CMS.ContentEngine.ChannelInfo>>();
            settingProviderMock = new Mock<IInfoProvider<ReportingChannelSettingInfo>>();
            localizerMock = new Mock<IStringLocalizer<SharedResources>>();
        }

        [Test]
        public void Type_Exists()
        {
            Assert.That(
                typeof(ReportingApplicationChannelSettingsListingPage),
                Is.Not.Null);
        }

        [Test]
        public void Class_Inherits_From_ListingPage()
        {
            Assert.That(
                typeof(ReportingApplicationChannelSettingsListingPage).BaseType?.Name,
                Is.EqualTo("ListingPage"));
        }

        [Test]
        public void Constructor_Has_Three_Parameters()
        {
            var ctor = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetConstructors()
                .Single();

            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(3));
        }

        [Test]
        public void Constructor_Parameter_1_Is_ChannelInfoProvider()
        {
            var ctor = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetConstructors()
                .Single();

            Assert.That(
                ctor.GetParameters()[0].ParameterType,
                Is.EqualTo(typeof(IInfoProvider<CMS.ContentEngine.ChannelInfo>)));
        }

        [Test]
        public void Constructor_Parameter_2_Is_ReportingChannelSettingInfoProvider()
        {
            var ctor = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetConstructors()
                .Single();

            Assert.That(
                ctor.GetParameters()[1].ParameterType,
                Is.EqualTo(typeof(IInfoProvider<ReportingChannelSettingInfo>)));
        }

        [Test]
        public void Constructor_Parameter_3_Is_StringLocalizer()
        {
            var ctor = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetConstructors()
                .Single();

            Assert.That(
                ctor.GetParameters()[2].ParameterType,
                Is.EqualTo(typeof(IStringLocalizer<SharedResources>)));
        }

        [Test]
        public void ObjectType_Property_Exists()
        {
            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(
                    "ObjectType",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null);
        }

        [Test]
        public void ObjectType_Returns_Expected_Value()
        {
            var page =
                (ReportingApplicationChannelSettingsListingPage)
                FormatterServices
                    .GetUninitializedObject(
                        typeof(ReportingApplicationChannelSettingsListingPage));

            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(
                    "ObjectType",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            var result = property?.GetValue(page);

            Assert.That(
                result,
                Is.EqualTo(ReportingChannelSettingInfo.OBJECT_TYPE));
        }

        [Test]
        public void EnsureSettingsListData_Method_Exists()
        {
            var method = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetMethod(
                    "EnsureSettingsListData",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void EnsureChannelSetting_Method_Exists()
        {
            var method = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetMethod(
                    "EnsureChannelSetting",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void EnsureChannelSettingDisplayName_Method_Exists()
        {
            var method = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetMethod(
                    "EnsureChannelSettingDisplayName",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void ConfigurePage_Method_Exists()
        {
            var method = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetMethod(nameof(ReportingApplicationChannelSettingsListingPage.ConfigurePage));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Assembly_Has_UIPage_Attribute()
        {
            var assembly = typeof(ReportingApplicationChannelSettingsListingPage).Assembly;

            var attribute = assembly
                .GetCustomAttributes(false)
                .FirstOrDefault(a => a.GetType().Name == "UIPageAttribute");

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void FormatterServices_Property_Exists()
        {
            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(nameof(ReportingApplicationChannelSettingsListingPage.FormatterServices));

            Assert.That(property, Is.Not.Null);
        }

        [Test]
        public void FormatterServices_Property_Has_Getter()
        {
            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(nameof(ReportingApplicationChannelSettingsListingPage.FormatterServices));

            Assert.That(property?.CanRead, Is.True);
        }

        [Test]
        public void FormatterServices_Property_Has_Setter()
        {
            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(nameof(ReportingApplicationChannelSettingsListingPage.FormatterServices));

            Assert.That(property?.CanWrite, Is.True);
        }
    }
}