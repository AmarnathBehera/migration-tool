using BDO.GWT.Core.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace BDO.GWT.Core.Entities
{
    public class LocalizationResource
    { 
     public string CultureCode { get; set; }
     public List<LocalizationKey> LocalizationKey { get; set; }
     public List<LocalizationValue> LocalizationValue { get; set; }
    }
    public class LocalizationKey
    {
     public int LocalizationKeyItemId { get; set; }
     public Guid LocalizationKeyItemGuid { get; set; }
     public string LocalizationKeyItemName { get; set; }
     public string LocalizationKeyItemDescription { get; set; }
    }
    public class LocalizationValue
    {
     public int LocalizationTranslationItemID { get; set; }
     public Guid LocalizationTranslationItemGuid { get; set; }
     public int LocalizationTranslationItemLocalizationKeyItemId { get; set; }
     public int LocalizationTranslationItemContentLanguageId { get; set; }
     public string LocalizationTranslationItemText { get; set; }
    }
}