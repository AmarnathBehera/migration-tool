//using CMS.ContentEngine;
//using CMS.DataEngine;
//using CMS.FormEngine;
//using CMS.Membership;
//using CMS.OnlineForms;
//using CMS.Websites;
//using CMS.Websites.Routing;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Xperience.EventRegistration
//{
//    public class EventRegistrationProvisioningService : IEventRegistrationProvisioningService
//    {
//        // TODO: replace with your actual Event Registration Page content type code name
//        private const string REGISTRATION_CONTENT_TYPE = "BDO.EventRegistrationPage";

//        // TODO: replace with the field code name on EventRegistrationPage that stores
//        // a reference/GUID to the BizForm to render on that page
//        private const string REGISTRATION_FORM_FIELD = "RegistrationFormGuid";

//        private readonly IWebPageManager webPageManager;
//        private readonly IContentQueryExecutor contentQueryExecutor;
//        private readonly IInfoProvider<BizFormInfo> bizFormInfoProvider;
//        private readonly IContentItemCodeNameProvider codeNameProvider;

//        public EventRegistrationProvisioningService(
//            IWebPageManagerFactory webPageManagerFactory,
//            IWebsiteChannelContext websiteChannelContext,
//            IUserInfoProvider userInfoProvider,
//            IContentQueryExecutor contentQueryExecutor,
//            IInfoProvider<BizFormInfo> bizFormInfoProvider,
//            IContentItemCodeNameProvider codeNameProvider)
//        {
//            // Uses the built-in system service account as the "author" of the automated changes
//            var systemUser = userInfoProvider.Get("kentico-system-service");

//            webPageManager = webPageManagerFactory.Create(websiteChannelContext.WebsiteChannelID, systemUser.UserID);
//            this.contentQueryExecutor = contentQueryExecutor;
//            this.bizFormInfoProvider = bizFormInfoProvider;
//            this.codeNameProvider = codeNameProvider;
//        }

//        public async Task EnsureRegistrationPageAsync(
//            int eventPageId,
//            string eventDisplayName,
//            string languageName,
//            CancellationToken cancellationToken)
//        {
//            // ---------------------------------------------------------------
//            // STEP 1: Idempotency guard.
//            // The publish event fires on every publish (including republishes
//            // and per-language-variant publishes). Since the event data itself
//            // doesn't tell us whether this is the very first publish, we check
//            // for an existing child registration page instead - functionally
//            // equivalent to "first time only", and safe against duplicate calls.
//            // ---------------------------------------------------------------
//            var existingRegistrationPageIds = await contentQueryExecutor.GetWebPageResult<int>(
//                new ContentItemQueryBuilder().ForContentType(REGISTRATION_CONTENT_TYPE, query =>
//                    query.Where(where => where.WhereEquals(
//                            nameof(WebPageFields.WebPageItemParentID), eventPageId))
//                         .TopN(1)),
//                row => row.WebPageItemID);

//            if (existingRegistrationPageIds.Any())
//            {
//                // Registration page + form already exist for this Event - nothing to do
//                return;
//            }

//            // ---------------------------------------------------------------
//            // STEP 2: Generate a unique, URL-safe code name from the Event's display name
//            // ---------------------------------------------------------------
//            string formCodeName = await codeNameProvider.Get($"{eventDisplayName} Registration Form");

//            // ---------------------------------------------------------------
//            // STEP 3: Build the form's field schema (First name, Last name,
//            // Email, Company, Job title)
//            // ---------------------------------------------------------------
//            FormInfo formInfo = new FormInfo(string.Empty);

//            formInfo.AddFormItem(new FormFieldInfo
//            {
//                Name = "FirstName",
//                DataType = FieldDataType.Text,
//                Size = 100,
//                Visible = true,
//                AllowEmpty = false,
//                Caption = "First name"
//            });

//            formInfo.AddFormItem(new FormFieldInfo
//            {
//                Name = "LastName",
//                DataType = FieldDataType.Text,
//                Size = 100,
//                Visible = true,
//                AllowEmpty = false,
//                Caption = "Last name"
//            });

//            formInfo.AddFormItem(new FormFieldInfo
//            {
//                Name = "Email",
//                DataType = FieldDataType.Text,
//                Size = 254,
//                Visible = true,
//                AllowEmpty = false,
//                Caption = "Email"
//            });

//            formInfo.AddFormItem(new FormFieldInfo
//            {
//                Name = "Company",
//                DataType = FieldDataType.Text,
//                Size = 200,
//                Visible = true,
//                AllowEmpty = true,
//                Caption = "Company"
//            });

//            formInfo.AddFormItem(new FormFieldInfo
//            {
//                Name = "JobTitle",
//                DataType = FieldDataType.Text,
//                Size = 200,
//                Visible = true,
//                AllowEmpty = true,
//                Caption = "Job title"
//            });

//            // ---------------------------------------------------------------
//            // STEP 4: Create the underlying DataClassInfo (the table + schema
//            // backing the form) and wrap it in a BizFormInfo
//            // ---------------------------------------------------------------
//            DataClassInfo dataClass = DataClassInfo.New();
//            dataClass.ClassName = formCodeName;
//            dataClass.ClassDisplayName = $"{eventDisplayName} Registration Form";
//            dataClass.ClassTableName = $"BDO_{formCodeName}";
//            dataClass.ClassIsForm = true;
//            dataClass.ClassFormDefinition = formInfo.GetXmlDefinition();
//            DataClassInfoProvider.SetDataClassInfo(dataClass);

//            BizFormInfo bizForm = new BizFormInfo
//            {
//                FormName = formCodeName,
//                FormDisplayName = $"{eventDisplayName} Registration Form",
//                FormClassID = dataClass.ClassID,

//            };
//            bizFormInfoProvider.Set(bizForm);

//            // ---------------------------------------------------------------
//            // STEP 5: Create the child "Event Registration" page under the
//            // Event, linking it to the newly created form
//            // ---------------------------------------------------------------
//            var itemData = new ContentItemData(new Dictionary<string, object>
//            {
//                { REGISTRATION_FORM_FIELD, bizForm.FormGUID }
//            });

//            var contentItemParameters = new ContentItemParameters(REGISTRATION_CONTENT_TYPE, itemData);

//            var createPageParameters = new CreateWebPageParameters(
//                $"{eventDisplayName} Registration",
//                languageName,
//                contentItemParameters)
//            {
//                // TODO: verify this member name against your installed SDK version -
//                // parent-page targeting on CreateWebPageParameters has shifted across
//                // releases, so confirm via IntelliSense before relying on it
//                ParentWebPageId = eventPageId
//            };

//            WebPageMetadata createdPage = await webPageManager.Create(createPageParameters);

//            // ---------------------------------------------------------------
//            // STEP 6: Publish the new registration page immediately so it's
//            // live alongside the Event itself
//            // ---------------------------------------------------------------
//            await webPageManager.TryPublish(createdPage.WebPageItemID, languageName);
//        }
//    }
//}