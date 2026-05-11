using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class PersonControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IPersonPageService> _personPageServiceMock = null!;
    private PersonController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _personPageServiceMock = new Mock<IPersonPageService>();
        _controller = new PersonController(_contentServiceMock.Object, _personPageServiceMock.Object);
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
            .Setup(s => s.RetrieveCurrentPageAsync<Person>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithExpectedTuple()
    {
        var page = new Person();
        var pageDto = new PersonDto { DisplayName = "John Doe" };
        var schemaDto = new PersonSchemaMarkupDto { Name = "John Doe", JobTitle = "Partner" };

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Person>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _personPageServiceMock
            .Setup(s => s.GetPersonPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageDto);

        _personPageServiceMock
            .Setup(s => s.GetPersonSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schemaDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var model = ((PersonDto, PersonSchemaMarkupDto))viewResult.Model!;
        Assert.Multiple(() =>
        {
            Assert.That(model.Item1, Is.EqualTo(pageDto));
            Assert.That(model.Item2, Is.EqualTo(schemaDto));
        });
    }

    [Test]
    public async Task Index_CallsServicesWithCorrectPage()
    {
        var page = new Person();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Person>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _personPageServiceMock
            .Setup(s => s.GetPersonPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDto { DisplayName = "Test" });

        _personPageServiceMock
            .Setup(s => s.GetPersonSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonSchemaMarkupDto());

        await _controller.Index(CancellationToken.None);

        _personPageServiceMock.Verify(s => s.GetPersonPageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
        _personPageServiceMock.Verify(s => s.GetPersonSchemaMarkupDto(page, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Index_DoesNotCallPageService_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Person>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        await _controller.Index(CancellationToken.None);

        _personPageServiceMock.Verify(s => s.GetPersonPageDtoAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        _personPageServiceMock.Verify(s => s.GetPersonSchemaMarkupDto(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}