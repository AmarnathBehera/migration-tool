using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Routing;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

public class DealCrudServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IValidationState> _validationStateMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<ICultureService> _cultureServiceMock = null!;
    private Mock<IInfoProvider<ChannelInfo>> _channelInfoProviderMock = null!;
    private Mock<IWebPageManagerFactory> _webPageManagerFactoryMock = null!;
    private Mock<IWebPageManager> _webPageManagerMock = null!;
    private Mock<IUserInfoProvider> _userInfoProviderMock = null!;
    private Mock<IMappingHelper> _mappingHelperMock = null!;
    private Mock<IPageContentValidator> _pageContentValidatorMock = null!;

    private DealCrudService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _validationStateMock = new Mock<IValidationState>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _cultureServiceMock = new Mock<ICultureService>();
        _channelInfoProviderMock = new Mock<IInfoProvider<ChannelInfo>>();
        _webPageManagerFactoryMock = new Mock<IWebPageManagerFactory>();
        _webPageManagerMock = new Mock<IWebPageManager>();
        _userInfoProviderMock = new Mock<IUserInfoProvider>();
        _mappingHelperMock = new Mock<IMappingHelper>();
        _pageContentValidatorMock = new Mock<IPageContentValidator>();

        _websiteChannelContextMock
            .SetupGet(x => x.WebsiteChannelID)
            .Returns(1);

        _userInfoProviderMock
            .Setup(x => x.Get("administrator"))
            .Returns(new UserInfo { UserID = 10 });

        _webPageManagerFactoryMock
            .Setup(x => x.Create(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(_webPageManagerMock.Object);

        _service = new DealCrudService(
            _contentServiceMock.Object,
            _validationStateMock.Object,
            _websiteChannelContextMock.Object,
            _cultureServiceMock.Object,
            _channelInfoProviderMock.Object,
            _webPageManagerFactoryMock.Object,
            _userInfoProviderMock.Object,
            _mappingHelperMock.Object,
            _pageContentValidatorMock.Object);
    }

    [Test]
    public void Constructor_CreatesServiceInstance()
    {
        Assert.That(_service, Is.Not.Null);
        Assert.That(_service, Is.InstanceOf<IDealCrudService>());
    }

    [Test]
    public async Task CreatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        ActionResult<PageOutput<DealPageContent>> result = await _service.CreatePageAsync(
            channelId: 1,
            nodeParentID: 10,
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());

        _pageContentValidatorMock.Verify(
            x => x.ValidateAndNormalizePageContent(It.IsAny<DealPageContent>()),
            Times.Never);
    }

    [Test]
    public async Task CreatePageCultureVersionAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        ActionResult<PageOutput<DealPageContent>> result = await _service.CreatePageCultureVersionAsync(
            channelId: 1,
            pageGUID: Guid.NewGuid(),
            fromCulture: "en-gb",
            toCulture: "en-us",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());

        _pageContentValidatorMock.Verify(
            x => x.ValidateAndNormalizePageContent(It.IsAny<DealPageContent>()),
            Times.Never);
    }

    [Test]
    public async Task UpdatePageAsync_WithNullRequest_ReturnsBadRequestResult()
    {
        ActionResult<PageOutput<DealPageContent>> result = await _service.UpdatePageAsync(
            channelId: 1,
            pageGUID: Guid.NewGuid(),
            culture: "en-gb",
            request: null!,
            cancellationToken: CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());

        _pageContentValidatorMock.Verify(
            x => x.ValidateAndNormalizePageContent(It.IsAny<DealPageContent>()),
            Times.Never);
    }
}
