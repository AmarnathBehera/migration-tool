using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class SectionPeoplePageControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ISectionPeoplePageService> _sectionPeoplePageServiceMock = null!;
    private SectionPeoplePageController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _sectionPeoplePageServiceMock = new Mock<ISectionPeoplePageService>();
        _controller = new SectionPeoplePageController(_contentServiceMock.Object, _sectionPeoplePageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsNotFound_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<SectionPeoplePage>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionPeoplePage?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithSectionPeopleDto()
    {
        var page = new SectionPeoplePage();
        var pageDto = new SectionPeopleDto
        {
            Hero = new HeroDto { Title = "Our People" },
            FilteringTooltipEnabled = true,
            PageSize = 12,
            ApiEndpoint = "/api/en/people"
        };

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<SectionPeoplePage>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _sectionPeoplePageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.EqualTo(pageDto));
    }

    [Test]
    public async Task Index_CallsServiceWithCorrectPage()
    {
        var page = new SectionPeoplePage();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<SectionPeoplePage>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _sectionPeoplePageServiceMock
            .Setup(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SectionPeopleDto
            {
                Hero = new HeroDto(),
                FilteringTooltipEnabled = false,
                PageSize = 10,
                ApiEndpoint = "/api/en/people"
            });

        await _controller.Index(CancellationToken.None);

        _sectionPeoplePageServiceMock.Verify(s => s.GetSectionPeoplePageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Index_DoesNotCallPageService_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<SectionPeoplePage>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionPeoplePage?)null);

        await _controller.Index(CancellationToken.None);

        _sectionPeoplePageServiceMock.Verify(s => s.GetSectionPeoplePageDtoAsync(It.IsAny<SectionPeoplePage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}