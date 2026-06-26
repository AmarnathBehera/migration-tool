using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface ICacheKeyPartsBuilder
{
    List<string> BuildCacheKeyParts();
}
