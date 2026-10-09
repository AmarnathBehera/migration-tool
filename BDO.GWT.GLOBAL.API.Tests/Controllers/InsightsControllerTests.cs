using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class InsightsControllerTests
{
    private Mock<IInsightCrudService> _insightCrudServiceMock = null!;
    private InsightsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _insightCrudServiceMock = new Mock<IInsightCrudService>();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 42;

        _controller = new InsightsController(_insightCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Test]
    public async Task Get_All_ReturnsOk_WithPageList()
    {
        var expected = new List<PageOutput<InsightPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<InsightPageContent>>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.GetInsightPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _insightCrudServiceMock.Verify(
            x => x.GetInsightPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithPage()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<InsightPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = new InsightPageContent()
        };
        var serviceResult = new ActionResult<PageOutput<InsightPageContent>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.GetInsightPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _insightCrudServiceMock.Verify(
            x => x.GetInsightPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ReturnsCreatedAtAction_WithNewPage()
    {
        var request = new InsightPageContent();
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<InsightPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<InsightPageContent>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(1, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _insightCrudServiceMock.Verify(
            x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateNewCultureVersion_ReturnsCreatedAtAction()
    {
        var pageGuid = Guid.NewGuid();
        var request = new InsightPageContent();
        var expected = new PageOutput<InsightPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<InsightPageContent>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateNewCultureVersion(1, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _insightCrudServiceMock.Verify(
            x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Put_ReturnsOk_WithUpdatedPage()
    {
        var pageGuid = Guid.NewGuid();
        var request = new InsightPageContent();
        var expected = new PageOutput<InsightPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<InsightPageContent>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(1, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _insightCrudServiceMock.Verify(
            x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_Success_ReturnsOk()
    {
        var pageGuid = Guid.NewGuid();

        _insightCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _insightCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsNotFound_OnNotFoundException()
    {
        var pageGuid = Guid.NewGuid();

        _insightCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Page not found"));
    }

    [Test]
    public async Task Delete_ReturnsUnprocessableEntity_OnUnprocessableEntityException()
    {
        var pageGuid = Guid.NewGuid();

        _insightCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task Delete_Returns500_OnException()
    {
        var pageGuid = Guid.NewGuid();

        _insightCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [TestCase(false)]
    [TestCase(null)]
    public async Task Get_All_ForwardsOnlyPublishedVariants_AndReturnsOk(bool? onlyPublished)
    {
        var expected = new List<PageOutput<InsightPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<InsightPageContent>>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.GetInsightPagesAsync(42, "en-gb", onlyPublished, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, "en-gb", onlyPublished, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _insightCrudServiceMock.Verify(
            x => x.GetInsightPagesAsync(42, "en-gb", onlyPublished, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateCopyInsight_ReturnsCreatedAtAction_WithNewContentItem()
    {
        var request = new CopyInsightPageContent { Title = "Copied", ImageFileAltText = "Alt" };
        var contentItemGuid = Guid.NewGuid();
        var expected = new PageOutput<CopyInsightPageContent>
        {
            General = new PageGeneralOutput { ContentItemGuid = contentItemGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<CopyInsightPageContent>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.CreateCopyInsightAsync(42, "en-gb", "KenticoDefault", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateCopyInsight(1, "en-gb", "KenticoDefault", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
            Assert.That(createdResult.RouteValues!["pageGuid"], Is.EqualTo(contentItemGuid));
        }

        _insightCrudServiceMock.Verify(
            x => x.CreateCopyInsightAsync(42, "en-gb", "KenticoDefault", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateCopyInsight_ForwardsChannelIdAndArguments_ToService()
    {
        var request = new CopyInsightPageContent { Title = "Copied", ImageFileAltText = "Alt" };
        var expected = new PageOutput<CopyInsightPageContent>
        {
            General = new PageGeneralOutput { ContentItemGuid = Guid.NewGuid() },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<CopyInsightPageContent>>(expected);

        _insightCrudServiceMock
            .Setup(x => x.CreateCopyInsightAsync(42, "fr-fr", "CustomWorkspace", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        await _controller.CreateCopyInsight(7, "fr-fr", "CustomWorkspace", request, CancellationToken.None);

        _insightCrudServiceMock.Verify(
            x => x.CreateCopyInsightAsync(42, "fr-fr", "CustomWorkspace", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateCopyInsight_WhenServiceReturnsBadRequest_PropagatesResult()
    {
        var request = new CopyInsightPageContent { Title = "Copied", ImageFileAltText = "Alt" };
        var serviceResult = new ActionResult<PageOutput<CopyInsightPageContent>>(new BadRequestResult());

        _insightCrudServiceMock
            .Setup(x => x.CreateCopyInsightAsync(42, "en-gb", "KenticoDefault", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateCopyInsight(1, "en-gb", "KenticoDefault", request, CancellationToken.None);

        // When the service returns a non-value ActionResult, Value is null so
        // CreatedAtAction is still produced with a null body.
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.That(createdResult.Value, Is.Null);
    }
}
