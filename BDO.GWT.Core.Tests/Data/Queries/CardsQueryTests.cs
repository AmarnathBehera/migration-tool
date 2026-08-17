using BDO.GWT.Core.Data.Queries;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Data.Queries;

[TestFixture]
public class CardsQueryTests
{
    [Test]
    public void Constructor_SetsExpectedDefaults()
    {
        var query = new CardsQuery();

        Assert.Multiple(() =>
        {
            Assert.That(query.Filters, Is.Not.Null.And.Empty);
            Assert.That(query.CurrentPageGuid, Is.Null);
            Assert.That(query.PageType, Is.Null);
            Assert.That(query.State, Is.Null);
            Assert.That(query.CurrentPage, Is.Null);
            Assert.That(query.PageSize, Is.Null);
            Assert.That(query.Order, Is.Null);
        });
    }

    [Test]
    public void State_CanBeAssigned()
    {
        var state = Guid.NewGuid();
        var query = new CardsQuery
        {
            State = state,
        };

        Assert.That(query.State, Is.EqualTo(state));
    }
}
