using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class SectionPeoplePageServiceTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<IPeopleCardsService> _peopleCardsServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private SectionPeoplePageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _heroServiceMock = new Mock<IHeroService>();
        _peopleCardsServiceMock = new Mock<IPeopleCardsService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _service = new SectionPeoplePageService(
            _contentServiceMock.Object,
            _heroServiceMock.Object,
            _peopleCardsServiceMock.Object,
            _cultureContextMock.Object);
    }

    [Test]
    public async Task GetSectionPeoplePersonPaginationDtoAsync_ReturnsPaginationDto()
    {
        var pageGuid = Guid.NewGuid();
        var cardsQuery = new CardsQuery();
        var sectionPage = new SectionPeoplePage();
        var expectedDto = new PaginationDto<PersonCardDto> { Data = [new() { DisplayName = "John" }], TotalCount = 1 };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage> { sectionPage });

        _peopleCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(sectionPage, cardsQuery, null, It.IsAny<Func<Person, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _service.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, cardsQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void GetSectionPeoplePersonPaginationDtoAsync_ThrowsMissingSectionPageException_WhenPageNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionPeoplePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionPeoplePage>());

        Assert.ThrowsAsync<MissingSectionPageException>(
            () => _service.GetSectionPeoplePersonPaginationDtoAsync(pageGuid, new CardsQuery(), CancellationToken.None));
    }
}