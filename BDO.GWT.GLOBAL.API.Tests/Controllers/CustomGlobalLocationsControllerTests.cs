using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Dto;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class CustomGlobalLocationsControllerTests
{
    private Mock<ICustomGlobalLocationsService> _serviceMock = null!;
    private CustomGlobalLocationsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<ICustomGlobalLocationsService>();

        _controller = new CustomGlobalLocationsController(_serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static GlobalLocationCustomTableApiDto CreateApiDto(int memberFirmId = 1) => new()
    {
        ItemId = 10,
        ItemGuid = Guid.NewGuid(),
        GlobalLocationTitle = "Head Office",
        MemberFirmId = memberFirmId,
        ChannelId = 42
    };

    private static GlobalLocationCustomTableUpsertDto CreateUpsertDto(int memberFirmId = 1) => new()
    {
        GlobalLocationTitle = "Head Office",
        MemberFirmId = memberFirmId,
        ChannelId = 42
    };

    [Test]
    public async Task Get_ReturnsOk_WithAllLocations()
    {
        var expected = new List<GlobalLocationCustomTableApiDto> { CreateApiDto() };

        _serviceMock
            .Setup(x => x.GetAllAsync(null))
            .ReturnsAsync(expected);

        var result = await _controller.Get(null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _serviceMock.Verify(x => x.GetAllAsync(null), Times.Once);
    }

    [Test]
    public async Task Get_WithMemberFirmId_ForwardsFilter_AndReturnsOk()
    {
        var expected = new List<GlobalLocationCustomTableApiDto> { CreateApiDto(5) };

        _serviceMock
            .Setup(x => x.GetAllAsync(5))
            .ReturnsAsync(expected);

        var result = await _controller.Get(5, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _serviceMock.Verify(x => x.GetAllAsync(5), Times.Once);
    }

    [Test]
    public async Task Create_ReturnsOk_WithCreatedLocation()
    {
        var request = CreateUpsertDto();
        var expected = CreateApiDto();

        _serviceMock
            .Setup(x => x.CreateAsync(request))
            .ReturnsAsync(expected);

        var result = await _controller.Create(request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _serviceMock.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Test]
    public async Task Create_ReturnsConflict_WhenDuplicateMemberFirm()
    {
        var request = CreateUpsertDto();

        _serviceMock
            .Setup(x => x.CreateAsync(request))
            .ThrowsAsync(new DuplicateMemberFirmException(request.MemberFirmId));

        var result = await _controller.Create(request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
        var conflict = (ConflictObjectResult)result.Result!;
        Assert.That(conflict.Value, Does.Contain(request.MemberFirmId.ToString()));
    }

    [Test]
    public async Task Create_Returns500_WhenPersistenceFails()
    {
        var request = CreateUpsertDto();

        _serviceMock
            .Setup(x => x.CreateAsync(request))
            .ThrowsAsync(new GlobalLocationPersistenceException("save failed", new Exception()));

        var result = await _controller.Create(request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task Update_ReturnsOk_WithUpdatedLocation()
    {
        const int memberFirmId = 1;
        var request = CreateUpsertDto(memberFirmId);
        var expected = CreateApiDto(memberFirmId);

        _serviceMock
            .Setup(x => x.UpdateAsync(memberFirmId, request))
            .ReturnsAsync(expected);

        var result = await _controller.Update(memberFirmId, request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _serviceMock.Verify(x => x.UpdateAsync(memberFirmId, request), Times.Once);
    }

    [Test]
    public async Task Update_ReturnsNotFound_WhenLocationDoesNotExist()
    {
        const int memberFirmId = 99;
        var request = CreateUpsertDto(memberFirmId);

        _serviceMock
            .Setup(x => x.UpdateAsync(memberFirmId, request))
            .ReturnsAsync((GlobalLocationCustomTableApiDto?)null);

        var result = await _controller.Update(memberFirmId, request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result.Result!;
        Assert.That(notFound.Value, Does.Contain(memberFirmId.ToString()));
    }

    [Test]
    public async Task Update_ReturnsConflict_WhenDuplicateMemberFirm()
    {
        const int memberFirmId = 1;
        var request = CreateUpsertDto(memberFirmId);

        _serviceMock
            .Setup(x => x.UpdateAsync(memberFirmId, request))
            .ThrowsAsync(new DuplicateMemberFirmException(memberFirmId));

        var result = await _controller.Update(memberFirmId, request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public async Task Update_Returns500_WhenPersistenceFails()
    {
        const int memberFirmId = 1;
        var request = CreateUpsertDto(memberFirmId);

        _serviceMock
            .Setup(x => x.UpdateAsync(memberFirmId, request))
            .ThrowsAsync(new GlobalLocationPersistenceException("update failed", new Exception()));

        var result = await _controller.Update(memberFirmId, request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }
}
