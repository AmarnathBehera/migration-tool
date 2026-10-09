using BDO.GWT.Domain.Core.Entities;

namespace BDO.GWT.Core.Tests.Entities;

public class MarketingContactTests
{
    [Test]
    public void Properties_CanBeAssignedAndRead()
    {
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid();

        var contact = new MarketingContact
        {
            ID = 10,
            FirstName = "John",
            MiddleName = "K",
            LastName = "Doe",
            Birthday = new DateTime(1990, 1, 1),
            Gender = 1,
            CompanyName = "BDO",
            JobTitle = "Manager",
            Created = now,
            StatusID = 2,
            Monitored = true,
            OwnerUserID = 99,
            Campaign = "Spring",
            PersonaID = 7,
            Address1 = "Line1",
            City = "Chicago",
            ZIP = "60601",
            StateID = 14,
            CountryID = 840,
            MobilePhone = "123456789",
            BusinessPhone = "987654321",
            Email = "john@bdo.com",
            Notes = "notes",
            GUID = guid,
            LastModified = now,
            Bounces = 3,
            SiteID = 5,
        };

        Assert.Multiple(() =>
        {
            Assert.That(contact.ID, Is.EqualTo(10));
            Assert.That(contact.FirstName, Is.EqualTo("John"));
            Assert.That(contact.MiddleName, Is.EqualTo("K"));
            Assert.That(contact.LastName, Is.EqualTo("Doe"));
            Assert.That(contact.Birthday, Is.EqualTo(new DateTime(1990, 1, 1)));
            Assert.That(contact.Gender, Is.EqualTo(1));
            Assert.That(contact.CompanyName, Is.EqualTo("BDO"));
            Assert.That(contact.JobTitle, Is.EqualTo("Manager"));
            Assert.That(contact.Created, Is.EqualTo(now));
            Assert.That(contact.StatusID, Is.EqualTo(2));
            Assert.That(contact.Monitored, Is.True);
            Assert.That(contact.OwnerUserID, Is.EqualTo(99));
            Assert.That(contact.Campaign, Is.EqualTo("Spring"));
            Assert.That(contact.PersonaID, Is.EqualTo(7));
            Assert.That(contact.Address1, Is.EqualTo("Line1"));
            Assert.That(contact.City, Is.EqualTo("Chicago"));
            Assert.That(contact.ZIP, Is.EqualTo("60601"));
            Assert.That(contact.StateID, Is.EqualTo(14));
            Assert.That(contact.CountryID, Is.EqualTo(840));
            Assert.That(contact.MobilePhone, Is.EqualTo("123456789"));
            Assert.That(contact.BusinessPhone, Is.EqualTo("987654321"));
            Assert.That(contact.Email, Is.EqualTo("john@bdo.com"));
            Assert.That(contact.Notes, Is.EqualTo("notes"));
            Assert.That(contact.GUID, Is.EqualTo(guid));
            Assert.That(contact.LastModified, Is.EqualTo(now));
            Assert.That(contact.Bounces, Is.EqualTo(3));
            Assert.That(contact.SiteID, Is.EqualTo(5));
        });
    }
}
