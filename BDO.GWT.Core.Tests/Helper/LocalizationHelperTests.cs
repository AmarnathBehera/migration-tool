using BDO.GWT.Core.Helper;
using CMS.ContentEngine;
using CMS.DataEngine;
using Moq;
using XperienceCommunity.Localization;

namespace BDO.GWT.Core.Tests.Helper;

public class LocalizationHelperTests
{
    private Mock<IInfoProvider<LocalizationKeyInfo>> _localizationKeyProviderMock = null!;
    private Mock<IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo>> _localizationTranslationProviderMock = null!;
    private Mock<IInfoProvider<ContentLanguageInfo>> _contentLanguageProviderMock = null!;
    private LocalizationHelper _service = null!;

    [SetUp]
    public void Setup()
    {
        _localizationKeyProviderMock = new Mock<IInfoProvider<LocalizationKeyInfo>>();
        _localizationTranslationProviderMock = new Mock<IInfoProvider<XperienceCommunity.Localization.LocalizationTranslationItemInfo>>();
        _contentLanguageProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();

        _service = new LocalizationHelper(
            _localizationKeyProviderMock.Object,
            _localizationTranslationProviderMock.Object,
            _contentLanguageProviderMock.Object);
    }

    // ── Interface contract ────────────────────────────────────────────────

    [Test]
    public void LocalizationHelper_ImplementsILocalizationHelper()
    {
        Assert.That(_service, Is.InstanceOf<ILocalizationHelper>());
    }

    // ── Key guard clause ──────────────────────────────────────────────────

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task GetTranslationAsync_WhenKeyIsNullOrWhitespace_ReturnsKeyAndDoesNotQuery(string? key)
    {
        var result = await _service.GetTranslationAsync("en-US", key!, CancellationToken.None);

        Assert.That(result, Is.EqualTo(key));
        _contentLanguageProviderMock.Verify(p => p.Get(), Times.Never);
        _localizationKeyProviderMock.Verify(p => p.Get(), Times.Never);
        _localizationTranslationProviderMock.Verify(p => p.Get(), Times.Never);
    }

    // ── Missing language / key branch ────────────────────────────────────

    [Test]
    public async Task GetTranslationAsync_WhenLanguageNotFound_ReturnsKey()
    {
        const string key = "Greeting.Hello";

        SetupContentLanguageQuery(Array.Empty<ContentLanguageInfo>());
        SetupLocalizationKeyQuery(Array.Empty<LocalizationKeyInfo>());

        var result = await _service.GetTranslationAsync("en-US", key, CancellationToken.None);

        Assert.That(result, Is.EqualTo(key));
        _localizationTranslationProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task GetTranslationAsync_WhenLocalizationKeyNotFound_ReturnsKey()
    {
        const string key = "Greeting.Hello";

        SetupContentLanguageQuery(new[] { CreateStub<ContentLanguageInfo>() });
        SetupLocalizationKeyQuery(Array.Empty<LocalizationKeyInfo>());

        var result = await _service.GetTranslationAsync("en-US", key, CancellationToken.None);

        Assert.That(result, Is.EqualTo(key));
        _localizationTranslationProviderMock.Verify(p => p.Get(), Times.Never);
    }

    // ── Error propagation ────────────────────────────────────────────────

    [Test]
    public void GetTranslationAsync_WhenProviderGetThrows_PropagatesException()
    {
        _contentLanguageProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("query unavailable"));
        _localizationKeyProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("query unavailable"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetTranslationAsync("en-US", "Greeting.Hello", CancellationToken.None));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void SetupContentLanguageQuery(IEnumerable<ContentLanguageInfo> results)
    {
        var queryMock = new Mock<ObjectQuery<ContentLanguageInfo>>();
        queryMock.Setup(q => q.WhereEquals(
                nameof(ContentLanguageInfo.ContentLanguageCultureFormat), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(1)).Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        _contentLanguageProviderMock.Setup(p => p.Get()).Returns(queryMock.Object);
    }

    private void SetupLocalizationKeyQuery(IEnumerable<LocalizationKeyInfo> results)
    {
        var queryMock = new Mock<ObjectQuery<LocalizationKeyInfo>>();
        queryMock.Setup(q => q.WhereEquals(
                nameof(LocalizationKeyInfo.LocalizationKeyItemName), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(1)).Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        _localizationKeyProviderMock.Setup(p => p.Get()).Returns(queryMock.Object);
    }

    private static T CreateStub<T>() =>
        (T)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(T));
}
