using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class DealsControllerTests
{
    private Mock<IDealCrudService> _dealCrudServiceMock = null!;
    private DealsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _dealCrudServiceMock = new Mock<IDealCrudService>();
        _controller = new DealsController(_dealCrudServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 42;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    [Test]
    public async Task GetDeals_ForwardsArguments_AndReturnsOk()
    {
        var expected = new List<PageOutput<DealPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<DealPageContent>>>(expected);

        _dealCrudServiceMock
            .Setup(x => x.GetDealPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _dealCrudServiceMock.Verify(
            x => x.GetDealPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetDealByGuid_ForwardsArguments_AndReturnsOk()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<DealPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = new DealPageContent { MetadataTitle = "Deal" },
        };

        var serviceResult = new ActionResult<PageOutput<DealPageContent>>(expected);

        _dealCrudServiceMock
            .Setup(x => x.GetDealPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _dealCrudServiceMock.Verify(
            x => x.GetDealPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ForwardsArguments_AndReturnsCreatedAtAction()
    {
        var request = new DealPageContent { MetadataTitle = "New deal" };
        var expected = new PageOutput<DealPageContent>
        {
            General = new PageGeneralOutput { PageGuid = Guid.NewGuid() },
            Content = request,
        };

        var serviceResult = new ActionResult<PageOutput<DealPageContent>>(expected);

        _dealCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(960, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _dealCrudServiceMock.Verify(
            x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Put_ForwardsArguments_AndReturnsOk()
    {
        var pageGuid = Guid.NewGuid();
        var request = new DealPageContent { MetadataTitle = "Updated deal" };
        var expected = new PageOutput<DealPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request,
        };

        var serviceResult = new ActionResult<PageOutput<DealPageContent>>(expected);

        _dealCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(960, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _dealCrudServiceMock.Verify(
            x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_ForwardsArguments_AndReturnsNoContent()
    {
        var pageGuid = Guid.NewGuid();

        _dealCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(960, pageGuid, "en-gb", true, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContentResult>());

        _dealCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
