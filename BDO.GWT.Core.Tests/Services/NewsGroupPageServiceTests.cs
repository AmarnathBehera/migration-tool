using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsGroupPageServiceTests
{
    private readonly Mock<INewsGroupPageService> _newsGroupPageServiceMock;

    public NewsGroupPageServiceTests()
    {
        _newsGroupPageServiceMock = new Mock<INewsGroupPageService>();
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_ReturnsExpectedDto()
    {
        var expectedDto = new NewsGroupDto
        {
            Title = "Group Title",
            Description = "Group Description",
            Links = new[]
            {
                new NewsGroupItemDto { Title = "Article 1", URL = "/article-1" },
                new NewsGroupItemDto { Title = "Article 2", URL = "/article-2" }
            }
        };

        _newsGroupPageServiceMock
            .Setup(x => x.GetNewsGroupDtoAsync(It.IsAny<NewsGroup>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsGroupPageServiceMock.Object.GetNewsGroupDtoAsync(
            new NewsGroup(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Group Title"));
            Assert.That(result.Description, Is.EqualTo("Group Description"));
            Assert.That(result.Links, Has.Length.EqualTo(2));
            Assert.That(result.Links[0].Title, Is.EqualTo("Article 1"));
        });
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_WhenNoLinks_ReturnsEmptyLinksArray()
    {
        var emptyDto = new NewsGroupDto
        {
            Title = "Group Title",
            Description = null,
            Links = Array.Empty<NewsGroupItemDto>()
        };

        _newsGroupPageServiceMock
            .Setup(x => x.GetNewsGroupDtoAsync(It.IsAny<NewsGroup>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        var result = await _newsGroupPageServiceMock.Object.GetNewsGroupDtoAsync(
            new NewsGroup(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Group Title"));
            Assert.That(result.Description, Is.Null);
            Assert.That(result.Links, Is.Empty);
        });
    }

    [Test]
    public async Task GetNewsGroupDtoAsync_InvokedOnce()
    {
        _newsGroupPageServiceMock
            .Setup(x => x.GetNewsGroupDtoAsync(It.IsAny<NewsGroup>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NewsGroupDto { Title = "Test", Links = [] });

        var page = new NewsGroup();
        await _newsGroupPageServiceMock.Object.GetNewsGroupDtoAsync(page, CancellationToken.None);

        _newsGroupPageServiceMock.Verify(
            x => x.GetNewsGroupDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
    }
}
