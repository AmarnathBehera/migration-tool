using BDO.GWT.Core.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace BDO.GWT.Core.Entities
{
    public class LocalizationResource
    { 
     public required string CultureCode { get; set; }
     public required List<LocalizationKey> LocalizationKey { get; set; }
     public required List<LocalizationValue> LocalizationValue { get; set; }
    }
    public class LocalizationKey
    {
     public int LocalizationKeyItemId { get; set; }
     public Guid LocalizationKeyItemGuid { get; set; }
     public required string LocalizationKeyItemName { get; set; } 
     public required string LocalizationKeyItemDescription { get; set; }
    }
    public class LocalizationValue
    {
     public int LocalizationTranslationItemID { get; set; }
     public Guid LocalizationTranslationItemGuid { get; set; }
     public int LocalizationTranslationItemLocalizationKeyItemId { get; set; }
     public int LocalizationTranslationItemContentLanguageId { get; set; }
     public required string LocalizationTranslationItemText { get; set; }
    }
}