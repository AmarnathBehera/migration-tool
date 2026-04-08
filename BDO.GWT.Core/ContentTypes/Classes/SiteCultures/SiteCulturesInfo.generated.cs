using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SiteCulturesInfo), SiteCulturesInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SiteCulturesInfo"/>.
    /// </summary>
    public partial class SiteCulturesInfo : AbstractInfo<SiteCulturesInfo, IInfoProvider<SiteCulturesInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdositelocalization.sitecultures";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SiteCulturesInfo>), OBJECT_TYPE, "BDOSiteLocalization.SiteCultures", "SiteCulturesID", "SiteCulturesLastModified", "SiteCulturesGuid", "SiteCultureCode", "SiteCultureCode", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Site cultures ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteCulturesID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteCulturesID)), 0);
            set => SetValue(nameof(SiteCulturesID), value);
        }


        /// <summary>
        /// Site culture code.
        /// </summary>
        [DatabaseField]
        public virtual string SiteCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteCultureCode)), String.Empty);
            set => SetValue(nameof(SiteCultureCode), value);
        }


        /// <summary>
        /// Site description.
        /// </summary>
        [DatabaseField]
        public virtual string SiteDescription
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteDescription)), String.Empty);
            set => SetValue(nameof(SiteDescription), value, String.Empty);
        }


        /// <summary>
        /// Site culture visible.
        /// </summary>
        [DatabaseField]
        public virtual bool SiteCultureVisible
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(SiteCultureVisible)), false);
            set => SetValue(nameof(SiteCultureVisible), value);
        }


        /// <summary>
        /// Selector abbreviation.
        /// </summary>
        [DatabaseField]
        public virtual string SelectorAbbreviation
        {
            get => ValidationHelper.GetString(GetValue(nameof(SelectorAbbreviation)), String.Empty);
            set => SetValue(nameof(SelectorAbbreviation), value, String.Empty);
        }


        /// <summary>
        /// Selector long form.
        /// </summary>
        [DatabaseField]
        public virtual string SelectorLongForm
        {
            get => ValidationHelper.GetString(GetValue(nameof(SelectorLongForm)), String.Empty);
            set => SetValue(nameof(SelectorLongForm), value, String.Empty);
        }


        /// <summary>
        /// Site font.
        /// </summary>
        [DatabaseField]
        public virtual string SiteFont
        {
            get => ValidationHelper.GetString(GetValue(nameof(SiteFont)), String.Empty);
            set => SetValue(nameof(SiteFont), value, String.Empty);
        }


        /// <summary>
        /// Site culture site ID.
        /// </summary>
        [DatabaseField]
        public virtual int SiteCultureSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteCultureSiteID)), 0);
            set => SetValue(nameof(SiteCultureSiteID), value, 0);
        }


        /// <summary>
        /// Site culture item order.
        /// </summary>
        [DatabaseField]
        public virtual int SiteCultureItemOrder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SiteCultureItemOrder)), 0);
            set => SetValue(nameof(SiteCultureItemOrder), value, 0);
        }


        /// <summary>
        /// Site cultures guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid SiteCulturesGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SiteCulturesGuid)), Guid.Empty);
            set => SetValue(nameof(SiteCulturesGuid), value);
        }


        /// <summary>
        /// Site cultures last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SiteCulturesLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(SiteCulturesLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(SiteCulturesLastModified), value);
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
        /// Creates an empty instance of the <see cref="SiteCulturesInfo"/> class.
        /// </summary>
        public SiteCulturesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SiteCulturesInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SiteCulturesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}