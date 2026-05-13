using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Web.Core.DTOs;
public class CareerContentDto : CareerDetailsDto
{
    public string? Content { get; set; }

    public string? ContactPerson { get; set; }
}
