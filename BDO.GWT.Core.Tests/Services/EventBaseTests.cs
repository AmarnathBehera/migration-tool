using BDO;
using CMS.ContentEngine;

namespace BDO.GWT.Core.Tests.Services
{
    public class EventBaseTests
    {
        [Test]
        public void ShowInWidget_ReturnsTrue_WhenEventIsNotPrivate()
        {
            var sut = new EventBase { EventIsPrivate = false };

            Assert.That(sut.ShowInWidget, Is.True);
        }

        [Test]
        public void ShowInWidget_ReturnsFalse_WhenEventIsPrivate()
        {
            var sut = new EventBase { EventIsPrivate = true };

            Assert.That(sut.ShowInWidget, Is.False);
        }

        [Test]
        public void RelatedContentDate_ReturnsNull_WhenEventDateFromIsDefault()
        {
            var sut = new EventBase { EventDateFrom = default };

            Assert.That(sut.RelatedContentDate, Is.Null);
        }

        [Test]
        public void RelatedContentDate_ReturnsEventDateFrom_WhenSet()
        {
            var date = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var sut = new EventBase { EventDateFrom = date };

            Assert.That(sut.RelatedContentDate, Is.EqualTo(date));
        }

        [Test]
        public void RelatedContentDateTo_ReturnsNull_WhenEventDateToIsDefault()
        {
            var sut = new EventBase { EventDateTo = default };

            Assert.That(sut.RelatedContentDateTo, Is.Null);
        }

        [Test]
        public void RelatedContentDateTo_ReturnsEventDateTo_WhenSet()
        {
            var date = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);
            var sut = new EventBase { EventDateTo = date };

            Assert.That(sut.RelatedContentDateTo, Is.EqualTo(date));
        }

        [Test]
        public void RelatedContentDateOverride_ReturnsEventDateOverride()
        {
            var sut = new EventBase { EventDateOverride = "Next week" };

            Assert.That(sut.RelatedContentDateOverride, Is.EqualTo("Next week"));
        }

        [Test]
        public void Image_GetSet_DelegatesTo_MetadataTeaserImage()
        {
            var sut = new EventBase();
            var attachments = new List<ContentAttachment>();

            sut.Image = attachments;

            Assert.Multiple(() =>
            {
                Assert.That(sut.MetadataTeaserImage, Is.SameAs(attachments));
                Assert.That(sut.Image, Is.SameAs(attachments));
            });
        }

        [Test]
        public void ImageAltText_GetSet_DelegatesTo_MetadataTeaserImageAltText()
        {
            var sut = new EventBase();

            sut.ImageAltText = "alt";

            Assert.Multiple(() =>
            {
                Assert.That(sut.MetadataTeaserImageAltText, Is.EqualTo("alt"));
                Assert.That(sut.ImageAltText, Is.EqualTo("alt"));
            });
        }

       
       
    }
}
