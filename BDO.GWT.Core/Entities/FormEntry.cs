using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Entities;

public class FormEntry : Dictionary<string, object?>, IEntity
{
    public int ID { get; set; }

    public int FormID { get; set; }

    public string FormCodeName { get; set; } = null!;
}
