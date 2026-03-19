

using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;

namespace BDO.GWT.Core.DTOs;
    public class HomeDto
    {
        public IEnumerable<HomeHeroSlideDto>? HomeHeroSlides { get; set; }

        public TrendingTopicsDto? TrendingTopics { get; set; }

        public MetaDataDto? MetaData { get; set; }

        public SearchAndSEODto? SearchAndSEO { get; set; }

        public OGMetaDataDto? OGMetaData { get; set; }

        public BaseContent? BaseContent { get; set; }

        public BaseWebPage? BaseWebPage { get; set; }


}
