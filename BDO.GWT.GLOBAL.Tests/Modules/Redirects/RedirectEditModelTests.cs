using System.Reflection;
using System.Runtime.Serialization;
using CMS.Websites;
using Moq;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectEditModelTests
{
    // ── structural ──────────────────────────────────────────────────────────

    [Test]
    public void Model_IsInternalClass()
    {
        Assert.That(typeof(RedirectEditModel).IsNotPublic, Is.True);
    }

    [Test]
    public void SourceUrl_PropertyExists_WithGetterAndSetter()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.SourceUrl));

        Assert.That(prop, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prop!.CanRead, Is.True);
            Assert.That(prop.CanWrite, Is.True);
        });
    }

    [Test]
    public void TargetWebPageItem_PropertyExists_WithGetterAndSetter()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.TargetWebPageItem));

        Assert.That(prop, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prop!.CanRead, Is.True);
            Assert.That(prop.CanWrite, Is.True);
        });
    }

    [Test]
    public void MapToRedirectInfo_MethodIsPublicAndAsync()
    {
        var method = typeof(RedirectEditModel).GetMethod(nameof(RedirectEditModel.MapToRedirectInfo));

        Assert.That(method, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(method!.IsPublic, Is.True);
            Assert.That(method.ReturnType, Is.EqualTo(typeof(Task)));
        });
    }

    // ── defaults ────────────────────────────────────────────────────────────

    [Test]
    public void Model_Defaults_SourceUrlToNull()
    {
        var model = new RedirectEditModel();

        Assert.That(model.SourceUrl, Is.Null);
    }

    [Test]
    public void Model_Defaults_TargetWebPageItemToEmptyCollection()
    {
        var model = new RedirectEditModel();

        Assert.That(model.TargetWebPageItem, Is.Not.Null);
        Assert.That(model.TargetWebPageItem, Is.Empty);
    }

    [Test]
    public void Model_TargetWebPageItem_CanBeReassigned()
    {
        var model = new RedirectEditModel
        {
            TargetWebPageItem = [new WebPageRelatedItem { WebPageGuid = Guid.NewGuid() }]
        };

        Assert.That(model.TargetWebPageItem.Count(), Is.EqualTo(1));
    }

    // ── MapToRedirectInfo — no target page ──────────────────────────────────

    [Test]
    public async Task MapToRedirectInfo_WhenTargetIsEmpty_SetsSourceUrlLowercase()
    {
        var model = new RedirectEditModel { SourceUrl = "/My/Custom/Path" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectSourceUrl, Is.EqualTo("/my/custom/path"));
    }

    [Test]
    public async Task MapToRedirectInfo_WhenTargetIsEmpty_ClearsTargetPageUrl()
    {
        var model = new RedirectEditModel { SourceUrl = "/path" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectTargetPageURL, Is.Empty);
    }

    [Test]
    public async Task MapToRedirectInfo_WhenTargetIsEmpty_LeavesTargetGuidAtDefault()
    {
        var model = new RedirectEditModel { SourceUrl = "/path" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectTargetWebPageItemGUID, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task MapToRedirectInfo_WhenTargetIsEmpty_LeavesChannelIdAtZero()
    {
        var model = new RedirectEditModel { SourceUrl = "/path" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectChannelID, Is.EqualTo(0));
    }

    [Test]
    public async Task MapToRedirectInfo_WhenTargetIsEmpty_LeavesChannelNameNull()
    {
        var model = new RedirectEditModel { SourceUrl = "/path" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectChannelName, Is.Null);
    }

    [Test]
    public async Task MapToRedirectInfo_WhenSourceIsNull_SetsSourceUrlToNull()
    {
        var model = new RedirectEditModel();
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectSourceUrl, Is.Null);
    }

    [Test]
    public async Task MapToRedirectInfo_WhenSourceIsEmpty_SetsSourceUrlToEmpty()
    {
        var model = new RedirectEditModel { SourceUrl = "" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectSourceUrl, Is.Empty);
    }

    [Test]
    public async Task MapToRedirectInfo_SourceUrl_IsAlwaysLowercase()
    {
        var model = new RedirectEditModel { SourceUrl = "/ABC/DEF" };
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectSourceUrl, Is.EqualTo("/abc/def"));
    }

    // ── RedirectEditModel – annotations ─────────────────────────────────────

    [Test]
    public void SourceUrl_HasRequiredValidationRuleAttribute()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.SourceUrl))!;
        var attr = prop.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "RequiredValidationRuleAttribute");

        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void TargetWebPageItem_HasRequiredValidationRuleAttribute()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.TargetWebPageItem))!;
        var attr = prop.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "RequiredValidationRuleAttribute");

        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void SourceUrl_HasTextInputComponentAttribute()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.SourceUrl))!;
        var attr = prop.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "TextInputComponentAttribute");

        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void TargetWebPageItem_HasWebPageSelectorComponentAttribute()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.TargetWebPageItem))!;
        var attr = prop.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "WebPageSelectorComponentAttribute");

        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void SourceUrl_PropertyType_IsNullableString()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.SourceUrl))!;

        Assert.That(prop.PropertyType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void TargetWebPageItem_PropertyType_IsIEnumerable()
    {
        var prop = typeof(RedirectEditModel).GetProperty(nameof(RedirectEditModel.TargetWebPageItem))!;

        Assert.That(typeof(IEnumerable<WebPageRelatedItem>).IsAssignableFrom(prop.PropertyType), Is.True);
    }

    // ── MapToRedirectInfo – multiple items in TargetWebPageItem uses first ───

   
    // ── MapToRedirectInfo – clears target URL before mapping ────────────────

    [Test]
    public async Task MapToRedirectInfo_AlwaysClearsTargetPageUrlFirst()
    {
        var info = MakeFakeInfo();
        info.RedirectTargetPageURL = "/old-url";

        var model = new RedirectEditModel { SourceUrl = "/new-source" };

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.That(info.RedirectTargetPageURL, Is.Empty);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static FakeRedirectInfo MakeFakeInfo() =>
        (FakeRedirectInfo)FormatterServices.GetUninitializedObject(typeof(FakeRedirectInfo));

    private sealed class FakeRedirectInfo : XperienceCommunity.Redirects.RedirectInfo
    {
        private string? _sourceUrl;
        private Guid _targetGuid;
        private string? _targetPageUrl;
        private string? _channelName;
        private int _channelId;

        public override string RedirectSourceUrl
        {
            get => _sourceUrl!;
            set => _sourceUrl = value;
        }

        public override Guid RedirectTargetWebPageItemGUID
        {
            get => _targetGuid;
            set => _targetGuid = value;
        }

        public override string RedirectTargetPageURL
        {
            get => _targetPageUrl ?? string.Empty;
            set => _targetPageUrl = value;
        }

        public override string RedirectChannelName
        {
            get => _channelName!;
            set => _channelName = value;
        }

        public override int RedirectChannelID
        {
            get => _channelId;
            set => _channelId = value;
        }
    }
}
