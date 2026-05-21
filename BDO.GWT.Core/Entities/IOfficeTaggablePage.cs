using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Entities;

public interface IOfficeTaggablePage : IPage
{
    public List<int> MetadataOfficeLocation { get; set; }
}
