using BDO.GWT.Core.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace BDO.GWT.Core.Entities
{
    public class Culture
    {
        public string Name { get; set; } = null!;

        public LanguageTag LanguageTag { get; set; } = null!;

        public string ShortName { get; set; } = null!;

        public Guid GUID { get; set; }

        public DateTime LastModified { get; set; }

        public string? Alias { get; set; }

        public bool IsUICulture { get; set; }
    }
}