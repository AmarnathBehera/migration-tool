
namespace BDO.GWT.Core.DTOs.SchemaMarkups;

public class FaqSchemaMarkupDto : BaseSchemaMarkupDto
{
    protected override string GetSchemaMarkupType() => "FAQPage";

    /// <summary>
    /// Gets or sets the MainEntity of the FAQ.
    /// </summary>
    public List<QuestionSchemaMarkupDto>? MainEntity { get; set; }
}
