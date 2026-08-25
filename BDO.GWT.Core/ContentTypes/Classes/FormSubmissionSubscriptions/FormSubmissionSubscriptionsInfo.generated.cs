using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(FormSubmissionSubscriptionsInfo), FormSubmissionSubscriptionsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="FormSubmissionSubscriptionsInfo"/>.
    /// </summary>
    public partial class FormSubmissionSubscriptionsInfo : AbstractInfo<FormSubmissionSubscriptionsInfo, IInfoProvider<FormSubmissionSubscriptionsInfo>>, IInfoWithId, IInfoWithName
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.formsubmissionsubscriptions";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FormSubmissionSubscriptionsInfo>), OBJECT_TYPE, "BDO.FormSubmissionSubscriptions", "FormSubmissionSubscriptionsID", null, null, "Codename", "Displayname", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
            },
        };


        /// <summary>
        /// Form submission subscriptions ID.
        /// </summary>
        [DatabaseField]
        public virtual int FormSubmissionSubscriptionsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FormSubmissionSubscriptionsID)), 0);
            set => SetValue(nameof(FormSubmissionSubscriptionsID), value);
        }


        /// <summary>
        /// Displayname.
        /// </summary>
        [DatabaseField]
        public virtual string Displayname
        {
            get => ValidationHelper.GetString(GetValue(nameof(Displayname)), String.Empty);
            set => SetValue(nameof(Displayname), value);
        }


        /// <summary>
        /// Codename.
        /// </summary>
        [DatabaseField]
        public virtual string Codename
        {
            get => ValidationHelper.GetString(GetValue(nameof(Codename)), String.Empty);
            set => SetValue(nameof(Codename), value);
        }


        /// <summary>
        /// Description.
        /// </summary>
        [DatabaseField]
        public virtual string Description
        {
            get => ValidationHelper.GetString(GetValue(nameof(Description)), String.Empty);
            set => SetValue(nameof(Description), value);
        }


        /// <summary>
        /// Form.
        /// </summary>
        [DatabaseField]
        [Obsolete("Property is deprecated and will be removed in the future. Use property Form instead.")]
        public virtual string mForm
        {
            get => ValidationHelper.GetString(GetValue(nameof(Form)), String.Empty);
            set => SetValue(nameof(Form), value);
        }


        /// <summary>
        /// Form.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public IEnumerable<string> Form
        {
            get => global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToModels<string>(GetValue(nameof(Form), String.Empty));
            set => SetValue(nameof(Form), global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToString<IEnumerable<string>>(value, Enumerable.Empty<string>(), null));
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
            get => ValidationHelper.GetString(GetValue(nameof(Method)), String.Empty);
            set => SetValue(nameof(Method), value);
        }


        /// <summary>
        /// Useauthorization.
        /// </summary>
        [DatabaseField]
        public virtual bool Useauthorization
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Useauthorization)), false);
            set => SetValue(nameof(Useauthorization), value);
        }


        /// <summary>
        /// Requestcontenttype.
        /// </summary>
        [DatabaseField]
        public virtual string Requestcontenttype
        {
            get => ValidationHelper.GetString(GetValue(nameof(Requestcontenttype)), String.Empty);
            set => SetValue(nameof(Requestcontenttype), value);
        }


        /// <summary>
        /// Requestbodyformat.
        /// </summary>
        [DatabaseField]
        public virtual string Requestbodyformat
        {
            get => ValidationHelper.GetString(GetValue(nameof(Requestbodyformat)), String.Empty);
            set => SetValue(nameof(Requestbodyformat), value);
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
            set => SetValue(nameof(Username), value, String.Empty);
        }


        /// <summary>
        /// Encrypted password.
        /// </summary>
        [DatabaseField]
        public virtual string EncryptedPassword
        {
            get => ValidationHelper.GetString(GetValue(nameof(EncryptedPassword)), String.Empty);
            set => SetValue(nameof(EncryptedPassword), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="FormSubmissionSubscriptionsInfo"/> class.
        /// </summary>
        public FormSubmissionSubscriptionsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FormSubmissionSubscriptionsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FormSubmissionSubscriptionsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}