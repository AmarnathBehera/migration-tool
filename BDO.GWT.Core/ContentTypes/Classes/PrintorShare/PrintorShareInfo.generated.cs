using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(PrintorShareInfo), PrintorShareInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="PrintorShareInfo"/>.
    /// </summary>
    public partial class PrintorShareInfo : AbstractInfo<PrintorShareInfo, IInfoProvider<PrintorShareInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.printorshare";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<PrintorShareInfo>), OBJECT_TYPE, "BDO.PrintorShare", "PrintorShareID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("PnSSettingsWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Printor share ID.
        /// </summary>
        [DatabaseField]
        public virtual int PrintorShareID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(PrintorShareID)), 0);
            set => SetValue(nameof(PrintorShareID), value);
        }


        /// <summary>
        /// Disable download.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableDownload
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableDownload)), false);
            set => SetValue(nameof(DisableDownload), value);
        }


        /// <summary>
        /// Disable print.
        /// </summary>
        [DatabaseField]
        public virtual bool DisablePrint
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisablePrint)), false);
            set => SetValue(nameof(DisablePrint), value);
        }


        /// <summary>
        /// Disable facebook.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableFacebook
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableFacebook)), false);
            set => SetValue(nameof(DisableFacebook), value);
        }


        /// <summary>
        /// Disable twitter.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableTwitter
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableTwitter)), false);
            set => SetValue(nameof(DisableTwitter), value);
        }


        /// <summary>
        /// Disable linked in.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableLinkedIn
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableLinkedIn)), false);
            set => SetValue(nameof(DisableLinkedIn), value);
        }


        /// <summary>
        /// Disable email.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableEmail
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableEmail)), false);
            set => SetValue(nameof(DisableEmail), value);
        }


        /// <summary>
        /// Disable copy link button.
        /// </summary>
        [DatabaseField]
        public virtual bool DisableCopyLinkButton
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisableCopyLinkButton)), false);
            set => SetValue(nameof(DisableCopyLinkButton), value);
        }


        /// <summary>
        /// Pn S settings web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int PnSSettingsWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(PnSSettingsWebChannelSettingID)), 0);
            set => SetValue(nameof(PnSSettingsWebChannelSettingID), value);
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
        /// Creates an empty instance of the <see cref="PrintorShareInfo"/> class.
        /// </summary>
        public PrintorShareInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="PrintorShareInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public PrintorShareInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}