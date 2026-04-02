using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SocialLinksInfo), SocialLinksInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SocialLinksInfo"/>.
    /// </summary>
    public partial class SocialLinksInfo : AbstractInfo<SocialLinksInfo, IInfoProvider<SocialLinksInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdosettings.sociallinks";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SocialLinksInfo>), OBJECT_TYPE, "BDOSettings.SocialLinks", "SocialLinksID", "SocialLinksLastModified", "SocialLinksGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Social links ID.
        /// </summary>
        [DatabaseField]
        public virtual int SocialLinksID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SocialLinksID)), 0);
            set => SetValue(nameof(SocialLinksID), value);
        }


        /// <summary>
        /// Social link url.
        /// </summary>
        [DatabaseField]
        public virtual string SocialLinkUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(SocialLinkUrl)), String.Empty);
            set => SetValue(nameof(SocialLinkUrl), value);
        }


        /// <summary>
        /// Social link alt text.
        /// </summary>
        [DatabaseField]
        public virtual string SocialLinkAltText
        {
            get => ValidationHelper.GetString(GetValue(nameof(SocialLinkAltText)), String.Empty);
            set => SetValue(nameof(SocialLinkAltText), value);
        }


        /// <summary>
        /// Social link icon type.
        /// </summary>
        [DatabaseField]
        public virtual string SocialLinkIconType
        {
            get => ValidationHelper.GetString(GetValue(nameof(SocialLinkIconType)), String.Empty);
            set => SetValue(nameof(SocialLinkIconType), value);
        }


        /// <summary>
        /// Social link custom icon.
        /// </summary>
        [DatabaseField]
        public virtual string SocialLinkCustomIcon
        {
            get => ValidationHelper.GetString(GetValue(nameof(SocialLinkCustomIcon)), String.Empty);
            set => SetValue(nameof(SocialLinkCustomIcon), value);
        }


        /// <summary>
        /// Social link order.
        /// </summary>
        [DatabaseField]
        public virtual int SocialLinkOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SocialLinkOrder)), 1);
            set => SetValue(nameof(SocialLinkOrder), value, 0);
        }


        /// <summary>
        /// Social link aria label.
        /// </summary>
        [DatabaseField]
        public virtual string SocialLinkAriaLabel
        {
            get => ValidationHelper.GetString(GetValue(nameof(SocialLinkAriaLabel)), String.Empty);
            set => SetValue(nameof(SocialLinkAriaLabel), value);
        }


        /// <summary>
        /// Social link culture code.
        /// </summary>
        [DatabaseField]
        public virtual string SocialLinkCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(SocialLinkCultureCode)), String.Empty);
            set => SetValue(nameof(SocialLinkCultureCode), value, String.Empty);
        }


        /// <summary>
        /// Social links guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SocialLinksGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SocialLinksGuid)), Guid.Empty);
            set => SetValue(nameof(SocialLinksGuid), value);
        }


        /// <summary>
        /// Social links last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SocialLinksLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SocialLinksLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SocialLinksLastModified), value);
        }


        /// <summary>
        /// Social link site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SocialLinkSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SocialLinkSiteID)), 0);
            set => SetValue(nameof(SocialLinkSiteID), value);
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
        /// Creates an empty instance of the <see cref="SocialLinksInfo"/> class.
        /// </summary>
        public SocialLinksInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SocialLinksInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SocialLinksInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}