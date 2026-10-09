namespace BDO.GWT.Core.Data
{
    public class FaqData
    {
        public string? Title { get; set; }

        public string Layout { get; set; } = string.Empty;

        public IEnumerable<Guid> PageGuids { get; set; } = [];

        public string HoverColour { get; set; } = string.Empty;

        public string TopMargin { get; set; } = string.Empty;

        public string BottomMargin { get; set; } = string.Empty;

        public string WidgetID { get; set; } = string.Empty;
    }
}