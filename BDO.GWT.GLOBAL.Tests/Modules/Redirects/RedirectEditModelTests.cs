using System.Reflection;
using System.Runtime.Serialization;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
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

    // ── MapToRedirectInfo — target page provided ─────────────────────────────

    [Test]
    public async Task MapToRedirectInfo_WhenTargetProvided_AssignsSourceAndTargetGuid()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(null);
        var guid = Guid.NewGuid();
        var model = ModelWithTarget(guid, "/Path");
        var info = MakeFakeInfo();

        await model.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

        Assert.Multiple(() =>
        {
            Assert.That(info.RedirectSourceUrl, Is.EqualTo("/path"));
            Assert.That(info.RedirectTargetWebPageItemGUID, Is.EqualTo(guid));
        });
    }

    [Test]
    public async Task MapToRedirectInfo_WhenWebPageItemNotFound_LeavesChannelAndUrlUnset()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(null);
        var info = MakeFakeInfo();

        await MapToRedirectInfo(info);

        Assert.Multiple(() =>
        {
            Assert.That(info.RedirectChannelID, Is.EqualTo(0));
            Assert.That(info.RedirectChannelName, Is.Null);
            Assert.That(info.RedirectTargetPageURL, Is.Empty);
        });
    }

    [Test]
    public async Task MapToRedirectInfo_WhenWebsiteChannelNotFound_LeavesChannelUnset()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(MakeWebPageItem(id: 3, websiteChannelId: 8));
        SetupWebsiteChannel(null);
        var info = MakeFakeInfo();

        await MapToRedirectInfo(info);

        Assert.Multiple(() =>
        {
            Assert.That(info.RedirectChannelID, Is.EqualTo(0));
            Assert.That(info.RedirectTargetPageURL, Is.Empty);
        });
    }

    [Test]
    public async Task MapToRedirectInfo_WhenChannelResolved_SetsChannelIdAndName()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(MakeWebPageItem(id: 3, websiteChannelId: 8));
        SetupWebsiteChannel(MakeWebsiteChannel(channelId: 4));
        SetupChannel(MakeChannel(id: 4, displayName: "Global"));
        SetupDefaultLanguage("en");

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(3, "en"))
            .ReturnsAsync(new WebPageUrl("~/target", "https://example.org/target"));

        var info = MakeFakeInfo();
        await MapToRedirectInfo(info, retriever.Object);

        Assert.Multiple(() =>
        {
            Assert.That(info.RedirectChannelID, Is.EqualTo(4));
            Assert.That(info.RedirectChannelName, Is.EqualTo("Global"));
            Assert.That(info.RedirectTargetPageURL, Is.EqualTo("/target"));
        });
    }

    [Test]
    public async Task MapToRedirectInfo_WhenChannelNull_UsesZeroIdButStillResolvesUrl()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(MakeWebPageItem(id: 3, websiteChannelId: 8));
        SetupWebsiteChannel(MakeWebsiteChannel(channelId: 4));
        SetupChannel(null);
        SetupDefaultLanguage("en");

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(3, "en"))
            .ReturnsAsync(new WebPageUrl("/no-channel", "https://example.org/no-channel"));

        var info = MakeFakeInfo();
        await MapToRedirectInfo(info, retriever.Object);

        Assert.Multiple(() =>
        {
            Assert.That(info.RedirectChannelID, Is.EqualTo(0));
            Assert.That(info.RedirectChannelName, Is.Null);
            Assert.That(info.RedirectTargetPageURL, Is.EqualTo("/no-channel"));
        });
    }

    [Test]
    public async Task MapToRedirectInfo_WhenDefaultLanguageMissing_LeavesTargetUrlEmpty()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(MakeWebPageItem(id: 3, websiteChannelId: 8));
        SetupWebsiteChannel(MakeWebsiteChannel(channelId: 4));
        SetupChannel(MakeChannel(id: 4, displayName: "Global"));
        SetupDefaultLanguage("   ");

        var info = MakeFakeInfo();
        await MapToRedirectInfo(info);

        Assert.Multiple(() =>
        {
            Assert.That(info.RedirectChannelID, Is.EqualTo(4));
            Assert.That(info.RedirectTargetPageURL, Is.Empty);
        });
    }

    [Test]
    public async Task MapToRedirectInfo_WhenRetrieveThrows_LeavesTargetUrlEmpty()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(MakeWebPageItem(id: 3, websiteChannelId: 8));
        SetupWebsiteChannel(MakeWebsiteChannel(channelId: 4));
        SetupChannel(MakeChannel(id: 4, displayName: "Global"));
        SetupDefaultLanguage("en");

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("no url"));

        var info = MakeFakeInfo();
        await MapToRedirectInfo(info, retriever.Object);

        Assert.That(info.RedirectTargetPageURL, Is.Empty);
    }

    [Test]
    public async Task MapToRedirectInfo_WhenResolvedRelativePathNull_LeavesTargetUrlEmpty()
    {
        KenticoContainerSetup.Reset();
        SetupWebPageItem(MakeWebPageItem(id: 3, websiteChannelId: 8));
        SetupWebsiteChannel(MakeWebsiteChannel(channelId: 4));
        SetupChannel(MakeChannel(id: 4, displayName: "Global"));
        SetupDefaultLanguage("en");

        var retriever = new Mock<IWebPageUrlRetriever>();
        retriever.Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new WebPageUrl(null!, "https://example.org"));

        var info = MakeFakeInfo();
        await MapToRedirectInfo(info, retriever.Object);

        Assert.That(info.RedirectTargetPageURL, Is.Empty);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static Task MapToRedirectInfo(FakeRedirectInfo info) =>
        new RedirectEditModel
        {
            SourceUrl = "/path",
            TargetWebPageItem = [new WebPageRelatedItem { WebPageGuid = Guid.NewGuid() }]
        }.MapToRedirectInfo(info, Mock.Of<IWebPageUrlRetriever>());

    private static Task MapToRedirectInfo(FakeRedirectInfo info, IWebPageUrlRetriever retriever) =>
        new RedirectEditModel
        {
            SourceUrl = "/path",
            TargetWebPageItem = [new WebPageRelatedItem { WebPageGuid = Guid.NewGuid() }]
        }.MapToRedirectInfo(info, retriever);

    private static RedirectEditModel ModelWithTarget(Guid guid, string sourceUrl) => new()
    {
        SourceUrl = sourceUrl,
        TargetWebPageItem = [new WebPageRelatedItem { WebPageGuid = guid }]
    };

    private static void SetupWebPageItem(WebPageItemInfo? item)
    {
        var q = new Mock<ObjectQuery<WebPageItemInfo>>();
        KenticoContainerSetup.WebPageItemProvider.Setup(p => p.Get()).Returns(q.Object);
        q.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(q.Object);
        q.Setup(x => x.TopN(It.IsAny<int>())).Returns(q.Object);
        q.Setup(x => x.GetEnumerator())
            .Returns(() => (item is null ? new List<WebPageItemInfo>() : new List<WebPageItemInfo> { item }).GetEnumerator());
    }

    private static void SetupWebsiteChannel(WebsiteChannelInfo? wc) =>
        KenticoContainerSetup.WebsiteChannelProvider.As<IInfoByIdProvider<WebsiteChannelInfo>>()
            .Setup(x => x.Get(It.IsAny<int>())).Returns(wc!);

    private static void SetupChannel(ChannelInfo? ch) =>
        KenticoContainerSetup.ChannelProvider.As<IInfoByIdProvider<ChannelInfo>>()
            .Setup(x => x.Get(It.IsAny<int>())).Returns(ch!);

    private static void SetupDefaultLanguage(string name)
    {
        var q = new Mock<ObjectQuery<ContentLanguageInfo>>();
        KenticoContainerSetup.ContentLanguageProvider.Setup(p => p.Get()).Returns(q.Object);
        q.Setup(x => x.TopN(It.IsAny<int>())).Returns(q.Object);
        q.Setup(x => x.Column(It.IsAny<string>())).Returns(q.Object);
        q.Setup(x => x.WhereTrue(It.IsAny<string>())).Returns(q.Object);
        q.Setup(x => x.GetScalarResult<string>(It.IsAny<string>())).Returns(name);
    }

    private static WebPageItemInfo MakeWebPageItem(int id, int websiteChannelId)
    {
        var i = (TestWebPageItemInfo)FormatterServices.GetUninitializedObject(typeof(TestWebPageItemInfo));
        i.WebPageItemID = id;
        i.WebPageItemWebsiteChannelID = websiteChannelId;
        return i;
    }

    private static WebsiteChannelInfo MakeWebsiteChannel(int channelId)
    {
        var i = (TestWebsiteChannelInfo)FormatterServices.GetUninitializedObject(typeof(TestWebsiteChannelInfo));
        i.WebsiteChannelChannelID = channelId;
        return i;
    }

    private static ChannelInfo MakeChannel(int id, string displayName)
    {
        var i = (TestChannelInfo)FormatterServices.GetUninitializedObject(typeof(TestChannelInfo));
        i.ChannelID = id;
        i.ChannelDisplayName = displayName;
        return i;
    }

    private sealed class TestWebPageItemInfo : WebPageItemInfo
    {
        public override int WebPageItemID { get; set; }
        public override int WebPageItemWebsiteChannelID { get; set; }
    }

    private sealed class TestWebsiteChannelInfo : WebsiteChannelInfo
    {
        public override int WebsiteChannelChannelID { get; set; }
    }

    private sealed class TestChannelInfo : ChannelInfo
    {
        public override int ChannelID { get; set; }
        public override string ChannelDisplayName { get; set; } = string.Empty;
    }

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
