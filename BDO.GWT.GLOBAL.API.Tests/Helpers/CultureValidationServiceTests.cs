using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Localization;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Helpers
{
    public class CultureValidationServiceTests
    {
        private Mock<IInfoProvider<ContentLanguageInfo>> _contentLanguageInfoProviderMock = null!;
        private CultureValidationService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentLanguageInfoProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();
            _service = new CultureValidationService(_contentLanguageInfoProviderMock.Object);
        }

        #region IsValidCulture – guard clauses (do not touch the provider)

        [Test]
        public void IsValidCulture_ReturnsFalse_WhenCultureCodeIsNull()
        {
            var result = _service.IsValidCulture(null!);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidCulture_ReturnsFalse_WhenCultureCodeIsEmpty()
        {
            var result = _service.IsValidCulture(string.Empty);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidCulture_ReturnsFalse_WhenCultureCodeIsWhitespace()
        {
            var result = _service.IsValidCulture("   ");

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidCulture_DoesNotCallProvider_WhenCultureCodeIsNullOrWhitespace()
        {
            // Get() is untouched for guard-clause inputs; verify it is never invoked.
            _service.IsValidCulture(null!);
            _service.IsValidCulture(string.Empty);
            _service.IsValidCulture("   ");

            _contentLanguageInfoProviderMock.Verify(x => x.Get(), Times.Never);
        }

        #endregion

        #region IsValidCulture / GetConfiguredLanguageCodes – provider interaction

        // NOTE: IInfoProvider<ContentLanguageInfo>.Get() returns CMS.DataEngine.ObjectQuery<ContentLanguageInfo>,
        // a concrete Kentico class tied to the CMS query/IoC infrastructure. It cannot be constructed or
        // chained (.Column().GetListResult<string>()) outside of a running CMS application, so it cannot be
        // stubbed to return a working, queryable result in isolation. To keep these tests meaningful without
        // a live CMS environment, we verify that the provider is actually invoked (and that exceptions from
        // it propagate) rather than asserting on the resulting configured language list.

        [Test]
        public void GetConfiguredLanguageCodes_InvokesProviderGet()
        {
            var marker = new InvalidOperationException("Get() was called");
            _contentLanguageInfoProviderMock
                .Setup(x => x.Get())
                .Throws(marker);

            var ex = Assert.Throws<InvalidOperationException>(() => _service.GetConfiguredLanguageCodes());

            Assert.That(ex, Is.SameAs(marker));
            _contentLanguageInfoProviderMock.Verify(x => x.Get(), Times.Once);
        }

        [Test]
        public void IsValidCulture_WhenCultureCodeIsNonEmpty_InvokesProviderGet()
        {
            var marker = new InvalidOperationException("Get() was called");
            _contentLanguageInfoProviderMock
                .Setup(x => x.Get())
                .Throws(marker);

            var ex = Assert.Throws<InvalidOperationException>(() => _service.IsValidCulture("en-US"));

            Assert.That(ex, Is.SameAs(marker));
            _contentLanguageInfoProviderMock.Verify(x => x.Get(), Times.Once);
        }

        #endregion

        #region Constructor

        [Test]
        public void Constructor_AllowsNullProvider()
        {
            // CultureValidationService does not null-check the injected provider.
            Assert.DoesNotThrow(() => new CultureValidationService(null!));
        }

        #endregion
    }
}