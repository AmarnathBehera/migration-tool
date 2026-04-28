using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;


public class CustomDropdownProperties : FormComponentProperties
{
    [TextAreaComponent(
        Label = "SQL Query",
        ExplanationText = "Enter a SELECT query. Single column = used for both value and text. Two columns = first is value, second is text. Or alias as 'Value' and 'Text'.",
        Order = 0)]
    public string Script { get; set; } = "";

}