namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingChannelSettingInfoTests
    {
        [Test]
        public void Type_Exists()
        {
            Assert.That(typeof(ReportingChannelSettingInfo), Is.Not.Null);
        }

        [Test]
        public void TypeInfo_IsInitialized()
        {
            Assert.That(
                ReportingChannelSettingInfo.TYPEINFO,
                Is.Not.Null);
        }
    }
}