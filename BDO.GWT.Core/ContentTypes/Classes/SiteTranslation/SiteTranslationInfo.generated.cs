using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SiteTranslationInfo), SiteTranslationInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SiteTranslationInfo"/>.
    /// </summary>
    public partial class SiteTranslationInfo : AbstractInfo<SiteTranslationInfo, IInfoProvider<SiteTranslationInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdositelocalization.sitetranslation";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SiteTranslationInfo>), OBJECT_TYPE, "BDOSiteLocalization.SiteTranslation", "SiteTranslationID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Site translation ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteTranslationID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteTranslationID)), 0);
            set => SetValue(nameof(SiteTranslationID), value);
        }


        /// <summary>
        /// Site translation site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteTranslationSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteTranslationSiteID)), 0);
            set => SetValue(nameof(SiteTranslationSiteID), value);
        }


        /// <summary>
        /// Site translation string ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteTranslationStringID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteTranslationStringID)), 0);
            set => SetValue(nameof(SiteTranslationStringID), value);
        }


        /// <summary>
        /// Site translation translation ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteTranslationTranslationID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteTranslationTranslationID)), 0);
            set => SetValue(nameof(SiteTranslationTranslationID), value);
        }


        /// <summary>
        /// Site translation string key.
        /// </summary>
        [DatabaseField]
        public virtual string SiteTranslationStringKey
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteTranslationStringKey)), String.Empty);
            set => SetValue(nameof(SiteTranslationStringKey), value);
        }


        /// <summary>
        /// Site translation culture code.
        /// </summary>
        [DatabaseField]
        public virtual string SiteTranslationCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteTranslationCultureCode)), String.Empty);
            set => SetValue(nameof(SiteTranslationCultureCode), value);
        }


        /// <summary>
        /// Site translation text.
        /// </summary>
        [DatabaseField]
        public virtual string SiteTranslationText
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteTranslationText)), String.Empty);
            set => SetValue(nameof(SiteTranslationText), value, String.Empty);
        }


        /// <summary>
        /// Site translation guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SiteTranslationGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SiteTranslationGuid)), Guid.Empty);
            set => SetValue(nameof(SiteTranslationGuid), value);
        }


        /// <summary>
        /// Site translation code name.
        /// </summary>
        [DatabaseField]
        public virtual string SiteTranslationCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteTranslationCodeName)), String.Empty);
            set => SetValue(nameof(SiteTranslationCodeName), value);
        }


        /// <summary>
        /// Site translation last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SiteTranslationLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SiteTranslationLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SiteTranslationLastModified), value);
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
        /// Creates an empty instance of the <see cref="SiteTranslationInfo"/> class.
        /// </summary>
        public SiteTranslationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SiteTranslationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SiteTranslationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}