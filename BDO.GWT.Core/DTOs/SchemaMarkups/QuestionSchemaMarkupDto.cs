namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class QuestionSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "Question";

    /// <summary>
    /// Gets or sets the Name of the Question.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the accepted answer for the current question.
    /// </summary>
    public AnswerSchemaMarkupDto? AcceptedAnswer { get; set; }
}
