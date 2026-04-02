using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(FooterLinksInfo), FooterLinksInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="FooterLinksInfo"/>.
    /// </summary>
    public partial class FooterLinksInfo : AbstractInfo<FooterLinksInfo, IInfoProvider<FooterLinksInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdosettings.footerlinks";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FooterLinksInfo>), OBJECT_TYPE, "BDOSettings.FooterLinks", "FooterLinksID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Footer links ID.
        /// </summary>
        [DatabaseField]
        public virtual int FooterLinksID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FooterLinksID)), 0);
            set => SetValue(nameof(FooterLinksID), value);
        }


        /// <summary>
        /// Footer link title.
        /// </summary>
        [DatabaseField]
        public virtual string FooterLinkTitle
        {
            get => ValidationHelper.GetString(GetValue(nameof(FooterLinkTitle)), String.Empty);
            set => SetValue(nameof(FooterLinkTitle), value);
        }


        /// <summary>
        /// Footer link url.
        /// </summary>
        [DatabaseField]
        public virtual string FooterLinkUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(FooterLinkUrl)), String.Empty);
            set => SetValue(nameof(FooterLinkUrl), value);
        }


        /// <summary>
        /// Footer link open new window.
        /// </summary>
        [DatabaseField]
        public virtual bool FooterLinkOpenNewWindow
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(FooterLinkOpenNewWindow)), false);
            set => SetValue(nameof(FooterLinkOpenNewWindow), value);
        }


        /// <summary>
        /// Footer link culture code.
        /// </summary>
        [DatabaseField]
        public virtual string FooterLinkCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(FooterLinkCultureCode)), String.Empty);
            set => SetValue(nameof(FooterLinkCultureCode), value, String.Empty);
        }


        /// <summary>
        /// Footer link order.
        /// </summary>
        [DatabaseField]
        public virtual int FooterLinkOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FooterLinkOrder)), 0);
            set => SetValue(nameof(FooterLinkOrder), value, 0);
        }


        /// <summary>
        /// Footer links guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FooterLinksGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(FooterLinksGuid)), Guid.Empty);
            set => SetValue(nameof(FooterLinksGuid), value);
        }


        /// <summary>
        /// Footer links last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FooterLinksLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(FooterLinksLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(FooterLinksLastModified), value);
        }


        /// <summary>
        /// Footer link site ID.
        /// </summary>
        [DatabaseField]
        public virtual int FooterLinkSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FooterLinkSiteID)), 0);
            set => SetValue(nameof(FooterLinkSiteID), value);
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
        /// Creates an empty instance of the <see cref="FooterLinksInfo"/> class.
        /// </summary>
        public FooterLinksInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FooterLinksInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FooterLinksInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}