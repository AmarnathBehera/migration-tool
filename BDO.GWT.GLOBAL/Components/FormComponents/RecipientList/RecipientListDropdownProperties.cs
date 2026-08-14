using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.GLOBAL.Components.FormComponents.RecipientList;

public class RecipientListDropdownProperties : FormComponentProperties<string>
{
    public RecipientListDropdownProperties() : base(FieldDataType.Text, 200)
    {
    }

    [TextInputComponent(Label = "Default value", Order = EditingComponentOrder.DEFAULT_VALUE)]
    public override string DefaultValue { get; set; } = string.Empty;
}
