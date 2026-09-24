using BDO.GWT.GLOBAL.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class NonAsciiUrlReplacementServiceTests
{
    private StubSettingProvider _settingProvider = null!;
    private INonAsciiUrlReplacementCache _cache = null!;
    private NonAsciiUrlReplacementService _service = null!;

    [SetUp]
    public void Setup()
    {
        _settingProvider = new StubSettingProvider();
        _cache = new PassThroughCache();
        _service = new NonAsciiUrlReplacementService(_settingProvider, _cache, NullLogger<NonAsciiUrlReplacementService>.Instance);
    }

    [TestCase("btpnewä", "btpnewae")]
    [TestCase("schön", "schoen")]
    [TestCase("über", "ueber")]
    [TestCase("groß", "gross")]
    [TestCase("plainascii", "plainascii")]
    public async Task ApplyReplacementsAsync_AppliesConfiguredMappings(string input, string expected)
    {
        _settingProvider.RawDictionary = "ä=ae;ö=oe;ü=ue;ß=ss";

        var result = await _service.ApplyReplacementsAsync(input, websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task ApplyReplacementsAsync_ReturnsOriginalInput_WhenInputIsNull()
    {
        _settingProvider.RawDictionary = "ä=ae";

        var result = await _service.ApplyReplacementsAsync(null, websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ApplyReplacementsAsync_ReturnsOriginalInput_WhenInputIsEmpty()
    {
        _settingProvider.RawDictionary = "ä=ae";

        var result = await _service.ApplyReplacementsAsync(string.Empty, websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task ApplyReplacementsAsync_ReturnsOriginalInput_WhenSettingIsMissing()
    {
        _settingProvider.RawDictionary = null;

        var result = await _service.ApplyReplacementsAsync("btpnewä", websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.EqualTo("btpnewä"));
    }

    [Test]
    public async Task ApplyReplacementsAsync_IgnoresMalformedEntries_AndStillAppliesValidMappings()
    {
        _settingProvider.RawDictionary = "invalid-entry;=;ä=ae;ö=oe";

        var result = await _service.ApplyReplacementsAsync("schön", websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.EqualTo("schoen"));
    }

    [Test]
    public async Task ApplyReplacementsAsync_UsesLatestDuplicateMappingValue()
    {
        _settingProvider.RawDictionary = "ä=ae;ä=aa";

        var result = await _service.ApplyReplacementsAsync("btpnewä", websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.EqualTo("btpnewaa"));
    }

    [Test]
    public async Task ApplyReplacementsAsync_UsesConfiguredUppercaseMappings()
    {
        _settingProvider.RawDictionary = "Ä=AE;Ö=OE;Ü=UE";

        var result = await _service.ApplyReplacementsAsync("BÄR", websiteChannelId: 42, CancellationToken.None);

        Assert.That(result, Is.EqualTo("BAER"));
    }

    [Test]
    public void ApplyRules_PrioritizesLongerKeysFirst()
    {
        var rules = NonAsciiUrlReplacementService.ParseRules("ä=ae;äe=aeX");

        var result = NonAsciiUrlReplacementService.ApplyRules("äe", rules);

        Assert.That(result.Value, Is.EqualTo("aeX"));
    }

    [Test]
    public void ParseRules_IgnoresEntriesSeparatedOnlyByNewlines()
    {
        var rules = NonAsciiUrlReplacementService.ParseRules("ä=ae\nö=oe");

        Assert.That(rules, Is.Empty);
    }

    private sealed class StubSettingProvider : INonAsciiUrlReplacementSettingProvider
    {
        public string? RawDictionary { get; set; }

        public Task<string?> GetRawDictionaryAsync(int websiteChannelId, CancellationToken cancellationToken)
            => Task.FromResult(RawDictionary);
    }

    private sealed class PassThroughCache : INonAsciiUrlReplacementCache
    {
        public Task<T> LoadAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CancellationToken cancellationToken)
            => valueFactory();
    }
}
