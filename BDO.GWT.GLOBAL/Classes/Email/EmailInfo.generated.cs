using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(EmailInfo), EmailInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="EmailInfo"/>.
    /// </summary>
    public partial class EmailInfo : AbstractInfo<EmailInfo, IInfoProvider<EmailInfo>>, IInfoWithId, IInfoWithName
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.email";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<EmailInfo>), OBJECT_TYPE, "BDO.Email", "EmailID", null, null, "DisplayName", "DisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("EmailSettingsWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Email ID.
        /// </summary>
        [DatabaseField]
        public virtual int EmailID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EmailID)), 0);
            set => SetValue(nameof(EmailID), value);
        }


        /// <summary>
        /// From email.
        /// </summary>
        [DatabaseField]
        public virtual string FromEmail
        {
            get => ValidationHelper.GetString(GetValue(nameof(FromEmail)), String.Empty);
            set => SetValue(nameof(FromEmail), value);
        }


        /// <summary>
        /// Email settings web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int EmailSettingsWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EmailSettingsWebChannelSettingID)), 0);
            set => SetValue(nameof(EmailSettingsWebChannelSettingID), value);
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
        /// Creates an empty instance of the <see cref="EmailInfo"/> class.
        /// </summary>
        public EmailInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="EmailInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public EmailInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}