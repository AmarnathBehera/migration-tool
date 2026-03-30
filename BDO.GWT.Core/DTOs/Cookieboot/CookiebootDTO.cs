using Kentico.Xperience.Admin.Base.FormAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Cookieboot
{
    public class CookiebotDTO
    {
        [CheckBoxComponent(Label = "Enable Cookie bot", Order = 1)]
        public bool EnableCookiebot { get; set; }

        [TextInputComponent(Label = "CookieID", Order = 2)]
        public string CookieID { get; set; }

        [CodeEditorComponent(Label = "Custom Cookiebot Script", Order = 3)]
        public string CustomCookiebotScript { get; set; }
    }
}
