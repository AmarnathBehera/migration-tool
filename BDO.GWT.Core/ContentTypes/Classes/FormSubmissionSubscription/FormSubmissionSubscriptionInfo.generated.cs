using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(FormSubmissionSubscriptionInfo), FormSubmissionSubscriptionInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="FormSubmissionSubscriptionInfo"/>.
    /// </summary>
    public partial class FormSubmissionSubscriptionInfo : AbstractInfo<FormSubmissionSubscriptionInfo, IInfoProvider<FormSubmissionSubscriptionInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdointegrations.formsubmissionsubscription";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FormSubmissionSubscriptionInfo>), OBJECT_TYPE, "BDOIntegrations.FormSubmissionSubscription", "FormSubmissionSubscriptionID", "FormSubmissionSubscriptionLastModified", "FormSubmissionSubscriptionGuid", "CodeName", "DisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("SiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Form submission subscription ID.
        /// </summary>
        [DatabaseField]
        public virtual int FormSubmissionSubscriptionID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FormSubmissionSubscriptionID)), 0);
            set => SetValue(nameof(FormSubmissionSubscriptionID), value);
        }


        /// <summary>
        /// Form submission subscription guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FormSubmissionSubscriptionGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(FormSubmissionSubscriptionGuid)), Guid.Empty);
            set => SetValue(nameof(FormSubmissionSubscriptionGuid), value);
        }


        /// <summary>
        /// Form submission subscription last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FormSubmissionSubscriptionLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(FormSubmissionSubscriptionLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(FormSubmissionSubscriptionLastModified), value);
        }


        /// <summary>
        /// Site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteID)), 0);
            set => SetValue(nameof(SiteID), value);
        }


        /// <summary>
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string DisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DisplayName)), String.Empty);
            set => SetValue(nameof(DisplayName), value);
        }


        /// <summary>
        /// Code name.
        /// </summary>
        [DatabaseField]
        public virtual string CodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CodeName)), String.Empty);
            set => SetValue(nameof(CodeName), value, String.Empty);
        }


        /// <summary>
        /// Description.
        /// </summary>
        [DatabaseField]
        public virtual string Description
        {
            get => ValidationHelper.GetString(GetValue(nameof(Description)), String.Empty);
            set => SetValue(nameof(Description), value, String.Empty);
        }


        /// <summary>
        /// Form.
        /// </summary>
        [DatabaseField]
        public virtual string Form
        {
            get => ValidationHelper.GetString(GetValue(nameof(Form)), String.Empty);
            set => SetValue(nameof(Form), value);
        }


        /// <summary>
        /// Endpoint.
        /// </summary>
        [DatabaseField]
        public virtual string Endpoint
        {
            get => ValidationHelper.GetString(GetValue(nameof(Endpoint)), String.Empty);
            set => SetValue(nameof(Endpoint), value);
        }


        /// <summary>
        /// Method.
        /// </summary>
        [DatabaseField]
        public virtual string Method
        {
            get => ValidationHelper.GetString(GetValue(nameof(Method)), "POST");
            set => SetValue(nameof(Method), value, String.Empty);
        }


        /// <summary>
        /// Use authorization.
        /// </summary>
        [DatabaseField]
        public virtual bool UseAuthorization
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(UseAuthorization)), false);
            set => SetValue(nameof(UseAuthorization), value);
        }


        /// <summary>
        /// Scheme.
        /// </summary>
        [DatabaseField]
        public virtual string Scheme
        {
            get => ValidationHelper.GetString(GetValue(nameof(Scheme)), "Basic");
            set => SetValue(nameof(Scheme), value, String.Empty);
        }


        /// <summary>
        /// Username.
        /// </summary>
        [DatabaseField]
        public virtual string Username
        {
            get => ValidationHelper.GetString(GetValue(nameof(Username)), String.Empty);
            set => SetValue(nameof(Username), value);
        }


        /// <summary>
        /// Encrypted password.
        /// </summary>
        [DatabaseField]
        public virtual string EncryptedPassword
        {
            get => ValidationHelper.GetString(GetValue(nameof(EncryptedPassword)), String.Empty);
            set => SetValue(nameof(EncryptedPassword), value);
        }


        /// <summary>
        /// Request content type.
        /// </summary>
        [DatabaseField]
        public virtual string RequestContentType
        {
            get => ValidationHelper.GetString(GetValue(nameof(RequestContentType)), String.Empty);
            set => SetValue(nameof(RequestContentType), value, String.Empty);
        }


        /// <summary>
        /// Request body format.
        /// </summary>
        [DatabaseField]
        public virtual string RequestBodyFormat
        {
            get => ValidationHelper.GetString(GetValue(nameof(RequestBodyFormat)), String.Empty);
            set => SetValue(nameof(RequestBodyFormat), value, String.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="FormSubmissionSubscriptionInfo"/> class.
        /// </summary>
        public FormSubmissionSubscriptionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FormSubmissionSubscriptionInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FormSubmissionSubscriptionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}