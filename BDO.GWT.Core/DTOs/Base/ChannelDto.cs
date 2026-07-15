using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.DTOs.Base
{
    public class ChannelDto
    {
        public int ChannelId { get; set; }
        public string? ChannelName { get; set; }

        public string? CurentLanguage {  get; set; }

        public bool isPreivew { get; set; } = false;
    }
}
