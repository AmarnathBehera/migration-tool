using CMS.Automation.Internal;
using CMS.AutomationEngine.Internal;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.EmailLibrary.Internal;
using CMS.FormEngine;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.DigitalMarketing.Internal;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;
using System.Text.Json;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IFormCrudService
{
    Task<IEnumerable<FormOutput>> GetFormsAsync(CancellationToken cancellationToken);
    Task<FormOutput> GetFormAsync(string formCodeName, CancellationToken cancellationToken);
    Task<IEnumerable<Dictionary<string, object?>>> GetFormEntriesAsync(string formCodeName, CancellationToken cancellationToken);
    Task<Dictionary<string, object?>> GetFormEntryAsync(string formCodeName, int formEntryID, CancellationToken cancellationToken);
    Task<FileStreamData> GetFormEntryAttachmentAsync(string formCodeName, int formEntryID, string formFileFieldName, CancellationToken cancellationToken);
    Task DeleteFormEntryAsync(string formCodeName, int formEntryID, CancellationToken cancellationToken);
}

public class FileStreamData
{
    public Stream Stream { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}

public class FormCrudService : IFormCrudService
{
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    private readonly IInfoProvider<EmailConfigurationInfo> _emailConfigurationProvider;
    private readonly IInfoProvider<EmailChannelInfo> _emailChannelProvider;
    private readonly IInfoProvider<EmailChannelSenderInfo> _emailChannelSenderProvider;
    private readonly IContentItemDataInfoRetriever _contentItemDataRetriever;
    private readonly IInfoProvider<ContentItemInfo> _contentItemProvider;
    private readonly IEmailChannelLanguageRetriever _languageRetriever;
    private readonly IEmailChannelSenderEmailProvider _senderEmailProvider;

    public FormCrudService(
        IInfoProvider<EmailConfigurationInfo> emailConfigurationProvider,
        IInfoProvider<EmailChannelInfo> emailChannelProvider,
        IInfoProvider<EmailChannelSenderInfo> emailChannelSenderProvider,
        IContentItemDataInfoRetriever contentItemDataRetriever,
        IInfoProvider<ContentItemInfo> contentItemProvider,
        IEmailChannelLanguageRetriever languageRetriever,
        IEmailChannelSenderEmailProvider senderEmailProvider)
    {
        _emailConfigurationProvider = emailConfigurationProvider;
        _emailChannelProvider = emailChannelProvider;
        _emailChannelSenderProvider = emailChannelSenderProvider;
        _contentItemDataRetriever = contentItemDataRetriever;
        _contentItemProvider = contentItemProvider;
        _languageRetriever = languageRetriever;
        _senderEmailProvider = senderEmailProvider;
    }

    public async Task<IEnumerable<FormOutput>> GetFormsAsync(CancellationToken cancellationToken)
    {
        var forms = await BizFormInfoProvider.ProviderObject
            .Get()
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var outputs = new List<FormOutput>();
        foreach (var form in forms)
            outputs.Add(await MapToFormOutputAsync(form, cancellationToken));

        return outputs;
    }

    public async Task<FormOutput> GetFormAsync(string formCodeName, CancellationToken cancellationToken)
    {
        var form = await GetValidatedFormAsync(formCodeName, cancellationToken);
        return await MapToFormOutputAsync(form, cancellationToken);
    }

    public async Task<IEnumerable<Dictionary<string, object?>>> GetFormEntriesAsync(string formCodeName, CancellationToken cancellationToken)
    {
        var form = await GetValidatedFormAsync(formCodeName, cancellationToken);

        var formClass = DataClassInfoProvider.GetDataClassInfo(form.FormClassID)
            ?? throw new InvalidOperationException($"Cannot find data class for form '{formCodeName}'.");

        var entries = await BizFormItemProvider.GetItems(formClass.ClassName)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return entries.Select(entry =>
            entry.ColumnNames.ToDictionary(col => col, col => (object?)entry.GetValue(col))
        );
    }

    public async Task<Dictionary<string, object?>> GetFormEntryAsync(string formCodeName, int formEntryID, CancellationToken cancellationToken)
    {
        var form = await GetValidatedFormAsync(formCodeName, cancellationToken);
        var formEntry = await GetValidatedFormEntryAsync(form, formEntryID, cancellationToken);
        return formEntry.ColumnNames.ToDictionary(col => col, col => (object?)formEntry.GetValue(col));
    }

    public async Task<FileStreamData> GetFormEntryAttachmentAsync(string formCodeName, int formEntryID, string formFileFieldName, CancellationToken cancellationToken)
    {
        var form = await GetValidatedFormAsync(formCodeName, cancellationToken);
        var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(form.FormClassID);

        var formFileFieldNames = BizFormInfoProvider.GetBizFormFileColumns(dataClassInfo.ClassFormDefinition).ToHashSet();

        if (formFileFieldNames.Count > 0 && !formFileFieldNames.Contains(formFileFieldName))
        {
            throw new InvalidOperationException(
                $"The form field '{formFileFieldName}' does not exist or its component is not a File uploader.");
        }

        var formEntry = await GetValidatedFormEntryAsync(form, formEntryID, cancellationToken);

        if (!formEntry.TryGetValue(formFileFieldName, out var formFieldValue) || formFieldValue is not string formFileFieldValue)
        {
            throw new InvalidOperationException(
                $"The form entry field '{formFileFieldName}' does not have any file uploaded.");
        }

        var parts = formFileFieldValue.Split('/');

        if (parts.Length < 2)
        {
            throw new InvalidOperationException(
                $"Unexpected file field value format for field '{formFileFieldName}'.");
        }

        var (internalFileName, friendlyFileName) = (parts[0], parts[1]);
        var filePhysicalPath = FormHelper.GetFilePhysicalPath(internalFileName);

        filePhysicalPath = filePhysicalPath.Replace("BDO.GWT.Global.API", "BDO.GWT.Global");

        var stream = File.OpenRead(filePhysicalPath);

        if (!_contentTypeProvider.TryGetContentType(friendlyFileName, out var contentType))
        {
            contentType = MediaTypeNames.Application.Octet;
        }

        var formEntryAttachment = new FileStreamData
        {
            Stream = stream,
            ContentType = contentType,
            FileName = friendlyFileName,
        };

        return formEntryAttachment;
    }

    public async Task DeleteFormEntryAsync(string formCodeName, int formEntryID, CancellationToken cancellationToken)
    {
        var form = await GetValidatedFormAsync(formCodeName, cancellationToken);
        var formEntry = await GetValidatedFormEntryAsync(form, formEntryID, cancellationToken);
        formEntry.Delete();
    }
    private static async Task<BizFormInfo> GetValidatedFormAsync(string formCodeName, CancellationToken cancellationToken)
    {
        var forms = await BizFormInfoProvider.ProviderObject
            .Get()
            .WhereEquals(nameof(BizFormInfo.FormName), formCodeName)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return forms.FirstOrDefault()
            ?? throw new System.Collections.Generic.KeyNotFoundException(
                $"Cannot find form with code name '{formCodeName}'.");
    }

    private static async Task<BizFormItem> GetValidatedFormEntryAsync(BizFormInfo form, int formEntryID, CancellationToken cancellationToken)
    {
        var formClass = DataClassInfoProvider.GetDataClassInfo(form.FormClassID)
            ?? throw new InvalidOperationException($"Cannot find data class for form '{form.FormName}'.");
        var formInfo = form.Form;
        var primaryKeyField = formInfo.GetFields<FormFieldInfo>()
            .FirstOrDefault(f => f.PrimaryKey)
            ?? throw new InvalidOperationException($"Cannot find primary key field for form '{form.FormName}'.");

        var entries = await BizFormItemProvider.GetItems(formClass.ClassName)
            .WhereEquals(primaryKeyField.Name, formEntryID)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return entries.FirstOrDefault()
            ?? throw new System.Collections.Generic.KeyNotFoundException(
                $"Cannot find form entry with ID '{formEntryID}'.");
    }

    private async Task<FormOutput> MapToFormOutputAsync(BizFormInfo form, CancellationToken cancellationToken)
    {
        return new FormOutput
        {
            General = new FormGeneralOutput
            {
                FormDisplayName = form.FormDisplayName,
                FormCodeName = form.FormName,
                SubmitButtonText = string.IsNullOrWhiteSpace(form.FormSubmitButtonText) ? null : form.FormSubmitButtonText,
                SubmitButtonImage = string.IsNullOrWhiteSpace(form.FormSubmitButtonImage) ? null : form.FormSubmitButtonImage,
                AfterTheFormIsSubmitted = AfterTheFormIsSubmitted.ReloadForm,
            },
            FormBuilder = string.IsNullOrWhiteSpace(form.FormBuilderLayout)
                ? null
                : JsonSerializer.Deserialize<object>(form.FormBuilderLayout),
            EmailNotification = null,
            Autoresponder = await GetAutoresponderOutputAsync(form.FormID, cancellationToken),
        };
    }

    private async Task<FormAutoresponderOutput?> GetAutoresponderOutputAsync(int formID, CancellationToken cancellationToken)
    {
        var triggers = await AbstractInfo<ObjectWorkflowTriggerInfo, IInfoProvider<ObjectWorkflowTriggerInfo>>
            .Provider.Get()
            .WhereEquals("TriggerObjectType", BizFormInfo.OBJECT_TYPE)
            .WhereEquals("TriggerTargetObjectID", formID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var trigger = triggers.FirstOrDefault();
        if (trigger is null) return null;

        var workflows = await AbstractInfo<WorkflowInfo, IInfoProvider<WorkflowInfo>>
            .Provider.Get()
            .WhereEquals("WorkflowID", trigger.TriggerWorkflowID)
            .WhereEquals("WorkflowAutomationType", (int)AutomationType.SimpleFormAutoresponder)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var workflow = workflows.FirstOrDefault();
        if (workflow is null) return null;

        int emailConfigId = workflow.GetIntegerValue("WorkflowEmailConfigurationID", 0);
        if (emailConfigId == 0) return null;

        var emailConfigs = await _emailConfigurationProvider
            .Get()
            .WhereEquals(nameof(EmailConfigurationInfo.EmailConfigurationID), emailConfigId)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var emailConfig = emailConfigs.FirstOrDefault();
        if (emailConfig is null) return null;

        try
        {
            var contentItem = await _contentItemProvider.GetAsync(emailConfig.EmailConfigurationContentItemID, cancellationToken);
            var contentLanguage = await _languageRetriever.GetEmailChannelLanguageInfoOrThrow(emailConfig.EmailConfigurationEmailChannelID, cancellationToken);
            var data = await _contentItemDataRetriever.GetContentItemData(contentItem, contentLanguage.ContentLanguageID, false, cancellationToken);
            var emailFields = new EmailContentTypeSpecificFieldValues(data);

            var emailChannel = (await _emailChannelProvider
                .Get()
                .WhereEquals(nameof(EmailChannelInfo.EmailChannelID), emailConfig.EmailConfigurationEmailChannelID)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault();

            string? senderEmail = null;
            string? senderDisplayName = null;

            if (emailChannel is not null && emailFields.EmailSenderID > 0)
            {
                var sender = await _emailChannelSenderProvider.GetAsync(emailFields.EmailSenderID);
                if (sender is not null)
                {
                    senderDisplayName = sender.EmailChannelSenderDisplayName;
                    senderEmail = await _senderEmailProvider.GetEmailAddress(emailChannel.EmailChannelID, sender.EmailChannelSenderName, cancellationToken);
                }
            }

            return new FormAutoresponderOutput
            {
                EmailConfigurationGuid = emailConfig.EmailConfigurationGUID,
                EmailName = emailConfig.EmailConfigurationName,
                Subject = emailFields.EmailSubject,
                SenderEmail = senderEmail,
                SenderDisplayName = senderDisplayName,
            };
        }
        catch
        {
            return new FormAutoresponderOutput
            {
                EmailConfigurationGuid = emailConfig.EmailConfigurationGUID,
                EmailName = emailConfig.EmailConfigurationName,
                Subject = null,
                SenderEmail = null,
                SenderDisplayName = null,
            };
        }
    }
}