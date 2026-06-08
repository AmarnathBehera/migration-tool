
namespace BDO.GWT.Core.Services
{

    public record CarouselState(string CurrentPageTypeName, int CurrentPageNodeId, string CurrentPageNodeAliasPath, HashSet<Guid> FeaturedNodeGuids)
    {
        public CarouselState() : this(string.Empty, 0, string.Empty, new HashSet<Guid>())
        {

        }
    }
}