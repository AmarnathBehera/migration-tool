using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Services;

public class FormOutput
{
    public FormGeneralOutput General { get; set; } = null!;
    public object? FormBuilder { get; set; }

    /// <summary>
    /// Always null in xByK. The email-notification feature (admin recipient emails
    /// on form submission) was removed from BizFormInfo and has no API replacement.
    /// </summary>
    public FormEmailNotificationOutput? EmailNotification { get; set; }

    /// <summary>
    /// Populated when the form's autoresponder is set to "Select email" mode in the
    /// admin UI, pointing at an Email channel email. Null when set to "None" or
    /// "Custom" (developer-coded via IFormSubmitAutomationEmailProvider).
    /// </summary>
    public FormAutoresponderOutput? Autoresponder { get; set; }
}

public enum AfterTheFormIsSubmitted
{
    DisplayText,
    RedirectToUrl,
    ReloadForm,
}

public class FormGeneralOutput
{
    public string FormDisplayName { get; set; } = null!;
    public string FormCodeName { get; set; } = null!;

    /// <summary>
    /// Always <see cref="AfterTheFormIsSubmitted.ReloadForm"/> in xByK.
    /// Post-submit redirect/display-text was removed from BizFormInfo.
    /// </summary>
    [System.ComponentModel.DefaultValue(AfterTheFormIsSubmitted.ReloadForm)]
    public AfterTheFormIsSubmitted AfterTheFormIsSubmitted { get; set; } = AfterTheFormIsSubmitted.ReloadForm;

    /// <summary>Not available in xByK — always null.</summary>
    public string? DisplayText { get; set; }

    /// <summary>Not available in xByK — always null.</summary>
    public string? RedirectToUri { get; set; }

    public string? SubmitButtonText { get; set; }
    public string? SubmitButtonImage { get; set; }
}

/// <summary>
/// Not used in xByK. Retained so consumers don't need contract changes.
/// </summary>
public class FormEmailNotificationOutput
{
    [Required]
    public string SenderEmail { get; set; } = null!;

    /// <summary>You can specify multiple addresses separated by semicolons.</summary>
    [Required]
    public string[] RecipientEmails { get; set; } = Array.Empty<string>();

    [Required]
    public string Subject { get; set; } = null!;

    [System.ComponentModel.DefaultValue(true)]
    public bool AttachUploadedDocuments { get; set; } = true;

    public string? CustomLayout { get; set; }
}

/// <summary>
/// In xByK the autoresponder is an Email channel email assigned to the form.
/// Subject and sender are resolved from the email's linked content item and
/// channel sender configuration.
/// </summary>
public class FormAutoresponderOutput
{
    /// <summary>
    /// Stable GUID of the EmailConfigurationInfo — safe for CI/CD serialisation.
    /// </summary>
    public Guid EmailConfigurationGuid { get; set; }

    /// <summary>Display name of the email as shown in the Email channel UI.</summary>
    public string EmailName { get; set; } = null!;

    /// <summary>
    /// Subject line from the email's content item (EmailContentTypeSpecificFieldValues.EmailSubject).
    /// Null if the email has no published version.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Sender email address resolved via IEmailChannelSenderEmailProvider.
    /// Null if the email has no published version or sender is not configured.
    /// </summary>
    public string? SenderEmail { get; set; }

    /// <summary>
    /// Sender display name from EmailChannelSenderInfo.EmailChannelSenderDisplayName.
    /// </summary>
    public string? SenderDisplayName { get; set; }
}