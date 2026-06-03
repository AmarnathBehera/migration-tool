using Kentico.Xperience.Admin.Base.FormAnnotations;


namespace BDO.GWT.Core.DTOs
{
    public class GlobalEventsWebinarDto
    {
        [CheckBoxComponent(Label = "Display location title in a sidebar", Order = 1)]
        public bool Displaylocationtitleinasidebar { get; set; }

        [NumberInputComponent(Label = "Minutes before the event/webinar to trigger calendar reminder", Order = 2)]
        public int Minutestotriggercalendarreminder { get; set; }

    }
}
