using BDO.GWT.Common.Core.Services;

namespace BDO.GWT.Core.Tests.Services;

public class DateTimeProviderTests
{
    [Test]
    public void Now_ReturnsCurrentSystemTime()
    {
        var provider = new DateTimeProvider();

        var before = DateTime.Now;
        var now = provider.Now;
        var after = DateTime.Now;

        Assert.That(now, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after));
    }
}
