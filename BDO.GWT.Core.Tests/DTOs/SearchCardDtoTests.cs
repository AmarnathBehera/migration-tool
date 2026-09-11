using System;
using System.Linq;
using System.Text.Json;

using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;

using NUnit.Framework;

namespace BDO.GWT.Core.Tests.DtoTests;

[TestFixture]
public class SearchCardDtoTests
{
    private const string INSIGHT_CARD_TYPE = "Insight";


    [Test]
    public void Data_WhenSet_IsExposedThroughDataObject()
    {
        var payload = new GenericSearchCardDto { Title = "Title", Description = "Desc" };

        var card = new SearchCardDto<GenericSearchCardDto>
        {
            CardType = "Generic",
            Data = payload
        };

        Assert.Multiple(() =>
        {
            Assert.That(card.Data, Is.SameAs(payload));
            Assert.That(card.DataObject, Is.SameAs(payload));
        });
    }


    [Test]
    public void Data_WhenNotSet_IsNull()
    {
        var card = new SearchCardDto<GenericSearchCardDto> { CardType = "Generic" };

        Assert.Multiple(() =>
        {
            Assert.That(card.Data, Is.Null);
            Assert.That(card.DataObject, Is.Null);
        });
    }


    [Test]
    public void Data_WhenSetToNull_ClearsDataObject()
    {
        var card = new SearchCardDto<GenericSearchCardDto>
        {
            CardType = "Generic",
            Data = new GenericSearchCardDto { Title = "Title" }
        };

        card.Data = null;

        Assert.That(card.DataObject, Is.Null);
    }


    [Test]
    public void SearchCardDto_IsAssignableToNonGenericBase()
    {
        SearchCardDto card = new SearchCardDto<InsightSearchCardDto>
        {
            CardType = INSIGHT_CARD_TYPE,
            Data = new InsightSearchCardDto { Title = "Insight" }
        };

        Assert.Multiple(() =>
        {
            Assert.That(card, Is.InstanceOf<SearchCardDto>());
            Assert.That(card.CardType, Is.EqualTo(INSIGHT_CARD_TYPE));
            Assert.That(card.DataObject, Is.InstanceOf<InsightSearchCardDto>());
        });
    }


    [Test]
    public void MixedCardTypes_CanBeStoredInSingleCollection()
    {
        SearchCardDto[] cards =
        [
            new SearchCardDto<InsightSearchCardDto> { CardType = INSIGHT_CARD_TYPE, Data = new InsightSearchCardDto { Title = "I" } },
            new SearchCardDto<EventWebinarSearchCard>
            {
                CardType = "EventsAndWebinars",
                Data = new EventWebinarSearchCard
                {
                    Title = "E",
                    Link = new LinkItemDto { Title = "Read more", URL = "/event-1" }
                }
            },
            new SearchCardDto<GenericSearchCardDto> { CardType = "Generic", Data = new GenericSearchCardDto { Title = "G" } }
        ];

        Assert.Multiple(() =>
        {
            Assert.That(cards.Select(x => x.CardType),
                Is.EqualTo(new[] { INSIGHT_CARD_TYPE, "EventsAndWebinars", "Generic" }));

            var eventCard = (EventWebinarSearchCard)cards[1].DataObject!;
            Assert.That(eventCard.Link, Is.InstanceOf<LinkItemDto>());
            Assert.That(eventCard.Link.URL, Is.EqualTo("/event-1"));
        });
    }


    [Test]
    public void Serialization_AsBaseType_WritesDataUsingLowercaseName()
    {
        SearchCardDto card = new SearchCardDto<GenericSearchCardDto>
        {
            CardType = "Generic",
            Data = new GenericSearchCardDto { Title = "Serialized title" }
        };

        var json = JsonSerializer.Serialize(card);

        using var document = JsonDocument.Parse(json);

        Assert.Multiple(() =>
        {
            Assert.That(document.RootElement.TryGetProperty("data", out _), Is.True, "Payload must be emitted as 'data'.");
            Assert.That(document.RootElement.GetProperty("CardType").GetString(), Is.EqualTo("Generic"));
        });
    }


    [Test]
    public void Serialization_WithNullData_EmitsNullPayload()
    {
        SearchCardDto card = new SearchCardDto<GenericSearchCardDto> { CardType = "Generic" };

        var json = JsonSerializer.Serialize(card);

        using var document = JsonDocument.Parse(json);

        Assert.That(document.RootElement.GetProperty("data").ValueKind, Is.EqualTo(JsonValueKind.Null));
    }
}


[TestFixture]
public class SearchCardTypeTests
{
    [Test]
    public void Enum_DefinesExpectedMembers()
    {
        var names = Enum.GetNames<SearchCardType>();

        Assert.That(names, Is.EquivalentTo(new[]
        {
            nameof(SearchCardType.Insight),
            nameof(SearchCardType.EventsAndWebinars),
            nameof(SearchCardType.Generic)
        }));
    }


    [Test]
    public void Default_IsInsight()
    {
        Assert.That(default(SearchCardType), Is.EqualTo(SearchCardType.Insight));
    }


    [TestCase(SearchCardType.Insight, "Insight")]
    [TestCase(SearchCardType.EventsAndWebinars, "EventsAndWebinars")]
    [TestCase(SearchCardType.Generic, "Generic")]
    public void ToString_MatchesCardTypeNameUsedByHelper(SearchCardType type, string expected)
    {
        Assert.That(type.ToString(), Is.EqualTo(expected));
    }


    [Test]
    public void AllValues_AreDistinct()
    {
        var values = Enum.GetValues<SearchCardType>().Cast<int>().ToArray();

        Assert.That(values, Is.Unique);
    }
}