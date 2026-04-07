using BDO.GWT.Web.Controllers;
using GWTXbyKWeb.Components.Services.SEOOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SEOControllerTests
    {
        private Mock<IOptionsSnapshot<SEOOptions>> _optionsMock;
        private SEOController _controller;

        [SetUp]
        public void Setup()
        {
            _optionsMock = new Mock<IOptionsSnapshot<SEOOptions>>();

            _controller = new SEOController(_optionsMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Index_ReturnsRobotsText()
        {
            // Arrange
            var options = new SEOOptions
            {
                RobotsText = "User-agent: *\nDisallow: /admin"
            };

            _optionsMock.Setup(x => x.Value).Returns(options);

            // Act
            var result = _controller.Index() as ContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("User-agent: *\nDisallow: /admin"));
            Assert.That(result.ContentType, Is.EqualTo("text/plain"));
        }

        [Test]
        public void Security_ReturnsSecurityText()
        {
            // Arrange
            var options = new SEOOptions
            {
                SecurityText = "Contact: security@example.com"
            };

            _optionsMock.Setup(x => x.Value).Returns(options);

            // Act
            var result = _controller.Security() as ContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("Contact: security@example.com"));
            Assert.That(result.ContentType, Is.EqualTo("text/plain"));
        }

        [Test]
        public void Index_ReturnsEmpty_WhenRobotsTextIsNull()
        {
            // Arrange
            var options = new SEOOptions
            {
                RobotsText = null
            };

            _optionsMock.Setup(x => x.Value).Returns(options);

            // Act
            var result = _controller.Index() as ContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Null);
            Assert.That(result.ContentType, Is.EqualTo("text/plain"));
        }

        [Test]
        public void Security_ReturnsEmpty_WhenSecurityTextIsNull()
        {
            // Arrange
            var options = new SEOOptions
            {
                SecurityText = null
            };

            _optionsMock.Setup(x => x.Value).Returns(options);

            // Act
            var result = _controller.Security() as ContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Null);
            Assert.That(result.ContentType, Is.EqualTo("text/plain"));
        }
    }
}