using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.Extensions.Localization;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingApplicationChannelSettingsListingPageTests
    {
        private Mock<IInfoProvider<ChannelInfo>> channelProviderMock = null!;
        private Mock<IInfoProvider<ReportingChannelSettingInfo>> settingProviderMock = null!;
        private Mock<IStringLocalizer<SharedResources>> localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            channelProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
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
                .GetMethod("ConfigurePage");

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Constructor_Has_Three_Parameters()
        {
            var ctor = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetConstructors()[0];

            Assert.That(ctor.GetParameters().Length, Is.EqualTo(3));
        }

        [Test]
        public void ObjectType_Returns_Expected_Value()
        {
            var page =
                (ReportingApplicationChannelSettingsListingPage)
                System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(
                        typeof(ReportingApplicationChannelSettingsListingPage));

            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(
                    "ObjectType",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null);

            var result = property!.GetValue(page);

            Assert.That(
                result,
                Is.EqualTo(ReportingChannelSettingInfo.OBJECT_TYPE));
        }


    }
}