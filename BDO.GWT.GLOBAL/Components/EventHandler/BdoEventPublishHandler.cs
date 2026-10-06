using AngleSharp.Dom;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.Websites;
using Kentico.Xperience.Admin.Base.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.EventHandler
{
    public class BdoEventPublishHandler : IAsyncEventHandler<AfterPublishWebPageEvent>
    {
        private readonly IWebPageManagerFactory webPageManagerFactory;
        private readonly IContentQueryExecutor contentQueryExecutor;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IContentItemCodeNameProvider contentItemCodeNameProvider;
        private readonly IInfoProvider<BizFormInfo> bizFormInfoProvider;
        private readonly IEventLogService eventLogService;
        private readonly IUserInfoProvider userInfoProvider;

        private const string EVENT_CONTENT_TYPE = "BDO.Event";
        private const string WEBINAR_CONTENT_TYPE = "BDO.Webinar";
        private const string REG_FORM_CONTENT_TYPE = "BDO.EventRegistrationForm";
        private const string TEMPLATE_FORM_CODE_NAME = "EventRegistrationTemplate";
        private const string FORM_FIELD_NAME = "Form";
        private const string META_TITLE_FIELD_NAME = "MetadataTitle";

        public BdoEventPublishHandler(
            IWebPageManagerFactory webPageManagerFactory,
            IContentQueryExecutor contentQueryExecutor,
           IHttpContextAccessor httpContextAccessor,
            IContentItemCodeNameProvider contentItemCodeNameProvider,
            IInfoProvider<BizFormInfo> bizFormInfoProvider,
            IEventLogService eventLogService,IUserInfoProvider userInfoProvider)
        {
            this.webPageManagerFactory = webPageManagerFactory;
            this.contentQueryExecutor = contentQueryExecutor;
            this.httpContextAccessor = httpContextAccessor;
            this.contentItemCodeNameProvider = contentItemCodeNameProvider;
            this.bizFormInfoProvider = bizFormInfoProvider;
            this.eventLogService = eventLogService;
            this.userInfoProvider = userInfoProvider;
        }

        public async Task HandleAsync(AfterPublishWebPageEvent asyncEvent, CancellationToken cancellationToken)
        {
            PublishWebPageEventData data = asyncEvent.Data;

            bool isEventOrWebinar =
                string.Equals(data.ContentTypeName, EVENT_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(data.ContentTypeName, WEBINAR_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase);

            if (!isEventOrWebinar)
            {
                return;
            }

            try
            {
                bool alreadyExists = await ChildRegistrationFormExistsAsync(
                    data.ID, data.WebsiteChannelName, data.ContentLanguageName, cancellationToken);

                if (alreadyExists)
                {
                    return;
                }

                var currentUser = GetCurrentUser();
               
                var webPageManager = webPageManagerFactory.Create(data.WebsiteChannelID, currentUser.UserID);
                BizFormInfo newForm = CreateEventRegistrationForm(data.DisplayName, DateTime.Now);
                string urlSlug = await contentItemCodeNameProvider.Get($"{data.DisplayName}-registration");

                var itemData = new ContentItemData(new Dictionary<string, object>
                {
                    { FORM_FIELD_NAME, newForm.FormName },
                    { META_TITLE_FIELD_NAME, data.DisplayName }
                });

                var contentItemParameters = new ContentItemParameters(REG_FORM_CONTENT_TYPE, itemData);
                var createPageParameters = new CreateWebPageParameters(data.DisplayName, data.ContentLanguageName, contentItemParameters)
                {
                    ParentWebPageItemID = data.ID,
                    UrlSlug = urlSlug
                };

                await webPageManager.Create(createPageParameters, cancellationToken);

                int newPageId = await GetChildRegistrationFormIdAsync(
                    data.ID, data.WebsiteChannelName, data.ContentLanguageName, cancellationToken);

                if (newPageId != 0)
                {
                    bool published = await webPageManager.TryPublish(newPageId, data.ContentLanguageName, cancellationToken);
                    if (!published)
                    {
                        eventLogService.LogEvent(
                            EventTypeEnum.Warning,
                            nameof(BdoEventPublishHandler),
                            "REGFORM_PUBLISH_FAILED",
                            $"Registration form page (ID {newPageId}) was created but could not be published automatically.");
                    }
                }
            }
            catch (UrlPathCollisionException ex)
            {
                eventLogService.LogException(
                    nameof(BdoEventPublishHandler),
                    "REGFORM_URL_COLLISION",
                    ex,
                    additionalMessage: $"Skipped auto-creating registration form for page ID {data.ID} due to a URL collision.");
            }
            catch (Exception ex)
            {
                eventLogService.LogException(
                    nameof(BdoEventPublishHandler),
                    "REGFORM_CREATE_FAILED",
                    ex,
                    additionalMessage: $"Failed to auto-create registration form/page for page ID {data.ID}.");
            }
        }
        private UserInfo GetCurrentUser()
        {
            var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return null;

            return userInfoProvider.Get(userName);
        }
        protected virtual BizFormInfo GetTemplateForm() => bizFormInfoProvider.Get(TEMPLATE_FORM_CODE_NAME);

        protected virtual BizFormInfo CreateEventRegistrationForm(string eventDisplayName, DateTime eventDate)
        {
            BizFormInfo templateForm = GetTemplateForm()
                ?? throw new InvalidOperationException(
                    $"Template form '{TEMPLATE_FORM_CODE_NAME}' was not found. Create it once in the Forms application (Name, Email, Phone fields) before this handler runs.");

            DataClassInfo templateClass = DataClassInfoProvider.GetDataClassInfo(templateForm.FormClassID);

            string formDisplayName = $"{eventDisplayName} {eventDate:yyyy-MM-dd}";
            string uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
            string newClassName = $"BDO.EventRegistration_{uniqueSuffix}";
            string newFormCodeName = $"EventRegistration_{uniqueSuffix}";

            DataClassInfo newClass = templateClass.Clone();
            newClass.ClassID = 0;
            newClass.ClassGUID = Guid.NewGuid();
            newClass.ClassName = newClassName;
            newClass.ClassTableName = newClassName.Replace(".", "_");
            newClass.ClassDisplayName = formDisplayName;
            DataClassInfoProvider.SetDataClassInfo(newClass);

            BizFormInfo newForm = templateForm.Clone();
            newForm.FormID = 0;
            newForm.FormGUID = Guid.NewGuid();
            newForm.FormClassID = newClass.ClassID;
            newForm.FormDisplayName = formDisplayName;
            newForm.FormName = newFormCodeName;
            bizFormInfoProvider.Set(newForm);

            return newForm;
        }

        private async Task<bool> ChildRegistrationFormExistsAsync(
            int parentPageId,
            string websiteChannelName,
            string languageName,
            CancellationToken cancellationToken)
        {
            int id = await GetChildRegistrationFormIdAsync(parentPageId, websiteChannelName, languageName, cancellationToken);
            return id != 0;
        }

        protected virtual async Task<int> GetChildRegistrationFormIdAsync(
            int parentPageId,
            string websiteChannelName,
            string languageName,
            CancellationToken cancellationToken)
        {
            var builder = new ContentItemQueryBuilder()
                .ForContentType(REG_FORM_CONTENT_TYPE, sub =>
                    sub.ForWebsite(websiteChannelName)
                       .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemParentID), parentPageId))
                       .TopN(1))
                .InLanguage(languageName);

            var result = await contentQueryExecutor.GetWebPageResult(
                builder,
                rowData => rowData.WebPageItemID,
                new ContentQueryExecutionOptions { ForPreview = true },
                cancellationToken);

            return result.FirstOrDefault();
        }
    }
}