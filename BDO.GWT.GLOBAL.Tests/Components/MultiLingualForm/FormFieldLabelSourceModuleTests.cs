using System.Reflection;
using BDO.Components.Widgets.FormWidget;
using CMS;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using XperienceCommunity.Localization;

namespace BDO.GWT.GLOBAL.Tests.Components.MultiLingualForm;

[TestFixture]
public class FormFieldLabelSourceModuleTests
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private FormFieldLabelSourceModule module = null!;

    [SetUp]
    public void SetUp() => module = new FormFieldLabelSourceModule();

    [TearDown]
    public void TearDown() => SetHttpContextAccessor(null);

    [Test]
    public void Module_InheritsFromModule()
    {
        Assert.That(typeof(CMS.DataEngine.Module).IsAssignableFrom(typeof(FormFieldLabelSourceModule)), Is.True);
    }

    [Test]
    public void Module_ExposesParameterlessConstructor()
    {
        var ctor = typeof(FormFieldLabelSourceModule).GetConstructor(Type.EmptyTypes);

        Assert.That(ctor, Is.Not.Null);
    }

    [Test]
    public void Module_HasExpectedModuleName()
    {
        Assert.That(module.Name, Is.EqualTo(nameof(FormFieldLabelSourceModule)));
    }

    [Test]
    public void Module_OverridesOnInit()
    {
        var onInit = typeof(FormFieldLabelSourceModule).GetMethod(
            "OnInit",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.That(onInit, Is.Not.Null);
        Assert.That(onInit!.DeclaringType, Is.EqualTo(typeof(FormFieldLabelSourceModule)));
    }

    [Test]
    public void Module_IsRegisteredViaAssemblyAttribute()
    {
        var registered = typeof(FormFieldLabelSourceModule).Assembly
            .GetCustomAttributes<RegisterModuleAttribute>()
            .Any(attribute => attribute.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Any(property => property.PropertyType == typeof(Type)
                    && (Type?)property.GetValue(attribute) == typeof(FormFieldLabelSourceModule)));

        Assert.That(registered, Is.True);
    }

    [Test]
    public void ResolveResourceString_ReturnsNull_WhenRequestServicesAreUnavailable()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(x => x.HttpContext).Returns(new DefaultHttpContext());
        SetHttpContextAccessor(accessor.Object);

        var result = InvokeResolveResourceString("bdo.form.title", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ResolveResourceString_ReturnsTranslation_ForExactCulture()
    {
        SetHttpContextAccessor(CreateAccessor(
            keys: [CreateLocalizationKey(10, "bdo.form.title")],
            languages: [CreateLanguage(20, "en-gb"), CreateLanguage(30, "en")],
            translations: [CreateTranslation(10, 20, "Title")]
        ));

        var result = InvokeResolveResourceString("bdo.form.title", "en-gb");

        Assert.That(result, Is.EqualTo("Title"));
    }

    [Test]
    public void ResolveResourceString_FallsBackToBaseLanguage_WhenSpecificCultureHasNoTranslation()
    {
        SetHttpContextAccessor(CreateAccessor(
            keys: [CreateLocalizationKey(10, "bdo.form.title")],
            languages: [CreateLanguage(20, "en-gb"), CreateLanguage(30, "en")],
            translations: [CreateTranslation(10, 30, "Base title")]
        ));

        var result = InvokeResolveResourceString("bdo.form.title", "en-gb");

        Assert.That(result, Is.EqualTo("Base title"));
    }

    [Test]
    public void ResolveResourceString_ReturnsNull_WhenLocalizationKeyDoesNotExist()
    {
        SetHttpContextAccessor(CreateAccessor(
            keys: [CreateLocalizationKey(10, "bdo.form.title")],
            languages: [CreateLanguage(20, "en-gb")],
            translations: [CreateTranslation(10, 20, "Title")]
        ));

        var result = InvokeResolveResourceString("Plain text label", "en-gb");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ResolveResourceString_ReturnsNull_WhenTranslationTextIsWhitespace()
    {
        SetHttpContextAccessor(CreateAccessor(
            keys: [CreateLocalizationKey(10, "bdo.form.title")],
            languages: [CreateLanguage(20, "en-gb")],
            translations: [CreateTranslation(10, 20, "   ")]
        ));

        var result = InvokeResolveResourceString("bdo.form.title", "en-gb");

        Assert.That(result, Is.Null);
    }

    private static IHttpContextAccessor CreateAccessor(
        IEnumerable<LocalizationKeyInfo> keys,
        IEnumerable<ContentLanguageInfo> languages,
        IEnumerable<LocalizationTranslationItemInfo> translations)
    {
        var keyProvider = CreateProvider(keys);
        var languageProvider = CreateProvider(languages);
        var translationProvider = CreateProvider(translations);

        var services = new ServiceCollection();
        services.AddSingleton(keyProvider.Object);
        services.AddSingleton(languageProvider.Object);
        services.AddSingleton(translationProvider.Object);

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(x => x.HttpContext).Returns(context);

        return accessor.Object;
    }

    private static Mock<IInfoProvider<T>> CreateProvider<T>(IEnumerable<T> rows)
        where T : AbstractInfoBase<T>, new()
    {
        var rowList = rows.ToList();
        var provider = new Mock<IInfoProvider<T>>();
        provider.Setup(x => x.Get()).Returns(() => CreateQuery(rowList).Object);

        return provider;
    }

    private static Mock<ObjectQuery<T>> CreateQuery<T>(IReadOnlyCollection<T> rows)
        where T : AbstractInfoBase<T>, new()
    {
        var filters = new List<(string Column, object Value)>();
        var query = new Mock<ObjectQuery<T>>();

        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Callback<string, object>((column, value) => filters.Add((column, value)))
            .Returns(query.Object);
        query.Setup(x => x.TopN(It.IsAny<int>())).Returns(query.Object);
        query.As<IEnumerable<T>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => rows.Where(row => MatchesFilters(row, filters)).GetEnumerator());

        return query;
    }

    private static bool MatchesFilters<T>(T row, IEnumerable<(string Column, object Value)> filters)
        where T : AbstractInfoBase<T>, new() =>
        filters.All(filter => ValuesEqual(GetPropertyValue(row, filter.Column), filter.Value));

    private static object? GetPropertyValue<T>(T row, string propertyName)
    {
        var property = row!.GetType().GetProperty(propertyName)
            ?? typeof(T).GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found on {typeof(T).Name}.");

        return property.GetValue(row);
    }

    private static bool ValuesEqual(object? actual, object expected)
    {
        if (actual is string actualText && expected is string expectedText)
        {
            return string.Equals(actualText, expectedText, StringComparison.OrdinalIgnoreCase);
        }

        return Equals(actual, Convert.ChangeType(expected, actual?.GetType() ?? expected.GetType()));
    }

    private string? InvokeResolveResourceString(string resourceKey, string languageName)
    {
        var method = typeof(FormFieldLabelSourceModule).GetMethod("ResolveResourceString", PrivateInstance);
        Assert.That(method, Is.Not.Null);

        return (string?)method!.Invoke(module, [resourceKey, languageName]);
    }

    private void SetHttpContextAccessor(IHttpContextAccessor? accessor)
    {
        var field = typeof(FormFieldLabelSourceModule).GetField("httpContextAccessor", PrivateInstance);
        Assert.That(field, Is.Not.Null);
        field!.SetValue(module, accessor);
    }

    private static TestLocalizationKeyInfo CreateLocalizationKey(int id, string itemName) => new()
    {
        LocalizationKeyItemId = id,
        LocalizationKeyItemName = itemName
    };

    private static TestContentLanguageInfo CreateLanguage(int id, string culture) => new()
    {
        ContentLanguageID = id,
        ContentLanguageCultureFormat = culture
    };

    private static TestLocalizationTranslationItemInfo CreateTranslation(int keyId, int languageId, string text) => new()
    {
        LocalizationTranslationItemLocalizationKeyItemId = keyId,
        LocalizationTranslationItemContentLanguageId = languageId,
        LocalizationTranslationItemText = text
    };

    private sealed class TestLocalizationKeyInfo : LocalizationKeyInfo
    {
        public override int LocalizationKeyItemId { get; set; }
        public override string LocalizationKeyItemName { get; set; } = string.Empty;
    }

    private sealed class TestContentLanguageInfo : ContentLanguageInfo
    {
        public override int ContentLanguageID { get; set; }
        public override string ContentLanguageCultureFormat { get; set; } = string.Empty;
    }

    private sealed class TestLocalizationTranslationItemInfo : LocalizationTranslationItemInfo
    {
        public override int LocalizationTranslationItemLocalizationKeyItemId { get; set; }
        public override int LocalizationTranslationItemContentLanguageId { get; set; }
        public override string LocalizationTranslationItemText { get; set; } = string.Empty;
    }
}