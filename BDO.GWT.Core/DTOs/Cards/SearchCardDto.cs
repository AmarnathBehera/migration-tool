using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BDO.GWT.Core.DTOs.Cards
{
    public abstract class SearchCardDto
    {
        public string CardType { get; set; } = null!;

        [JsonPropertyName("data")]
        public object? DataObject { get; protected set; }
    }

    public class SearchCardDto<TCard> : SearchCardDto
    {
        public TCard? Data
        {
            get => (TCard?)DataObject;
            set => DataObject = value;
        }
    }
}
