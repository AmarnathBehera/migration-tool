using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GDPRConsentInfo), GDPRConsentInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GDPRConsentInfo"/>.
    /// </summary>
    public partial class GDPRConsentInfo : AbstractInfo<GDPRConsentInfo, IInfoProvider<GDPRConsentInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.gdpr_consent";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GDPRConsentInfo>), OBJECT_TYPE, "BDO.GDPRConsent", "GDPR_ConsentID", "GDPR_ConsentLastModified", "GDPR_ConsentGuid", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Consent ID.
        /// </summary>
        [DatabaseField]
        public virtual int GDPR_ConsentID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GDPR_ConsentID)), 0);
            set => SetValue(nameof(GDPR_ConsentID), value);
        }


        /// <summary>
        /// Consent guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GDPR_ConsentGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(GDPR_ConsentGuid)), Guid.Empty);
            set => SetValue(nameof(GDPR_ConsentGuid), value);
        }


        /// <summary>
        /// Consent content.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentContent
        {
            get => ValidationHelper.GetString(GetValue(nameof(ConsentContent)), String.Empty);
            set => SetValue(nameof(ConsentContent), value);
        }


        /// <summary>
        /// Consent site ID.
        /// </summary>
        [DatabaseField]
        public virtual int ConsentSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ConsentSiteID)), 0);
            set => SetValue(nameof(ConsentSiteID), value);
        }


        /// <summary>
        /// Consent culture.
        /// </summary>
        [DatabaseField]
        public virtual string ConsentCulture
        {
            get => ValidationHelper.GetString(GetValue(nameof(ConsentCulture)), String.Empty);
            set => SetValue(nameof(ConsentCulture), value);
        }


        /// <summary>
        /// Consent last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GDPR_ConsentLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(GDPR_ConsentLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(GDPR_ConsentLastModified), value);
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
        /// Creates an empty instance of the <see cref="GDPRConsentInfo"/> class.
        /// </summary>
        public GDPRConsentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GDPRConsentInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GDPRConsentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}