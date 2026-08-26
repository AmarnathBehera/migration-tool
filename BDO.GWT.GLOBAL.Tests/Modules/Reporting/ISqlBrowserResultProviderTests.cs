using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    public class ISqlBrowserResultProviderTests
    {
        [Test]
        public void Type_ISqlBrowserResultProvider_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ISqlBrowserResultProvider");
            Assert.That(t, Is.Not.Null);
            Assert.That(t.IsInterface, Is.True);
        }
    }
}
