using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class SectionRssControllerTests
{
    private Mock<IRssSectionPageService> _rssSectionPageServiceMock;
    private SectionRssController _sut;

    [SetUp]
    public void SetUp()
    {
        _rssSectionPageServiceMock = new Mock<IRssSectionPageService>();
        _sut = new SectionRssController(_rssSectionPageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        var dto = new RssSectionDto();
        _rssSectionPageServiceMock
            .Setup(s => s.GetRssSectionDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_PassesDtoAsViewModel()
    {
        var dto = new RssSectionDto();
        _rssSectionPageServiceMock
            .Setup(s => s.GetRssSectionDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Index(CancellationToken.None) as ViewResult;

        Assert.That(result!.Model, Is.SameAs(dto));
    }

    [Test]
    public async Task Index_CallsServiceOnce()
    {
        _rssSectionPageServiceMock
            .Setup(s => s.GetRssSectionDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RssSectionDto());

        await _sut.Index(CancellationToken.None);

        _rssSectionPageServiceMock.Verify(
            s => s.GetRssSectionDtoAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    

    [Test]
    public async Task Index_PropagatesCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _rssSectionPageServiceMock
            .Setup(s => s.GetRssSectionDtoAsync(token))
            .ReturnsAsync(new RssSectionDto());

        await _sut.Index(token);

        _rssSectionPageServiceMock.Verify(
            s => s.GetRssSectionDtoAsync(token),
            Times.Once);
    }
}

