namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class AnswerSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Answer";

    /// <summary>
    /// Gets or sets the Text of the Answer.
    /// </summary>
    public string? Text { get; set; }
}
