using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(SEOSettingsInfo), SEOSettingsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="SEOSettingsInfo"/>.
    /// </summary>
    public partial class SEOSettingsInfo : AbstractInfo<SEOSettingsInfo, IInfoProvider<SEOSettingsInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.seosettings";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SEOSettingsInfo>), OBJECT_TYPE, "BDO.SEOSettings", "SEOSettingsID", null, "SeoSettingsGUID", null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("SeoSettingsWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// SEO settings ID.
        /// </summary>
        [DatabaseField]
        public virtual int SEOSettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SEOSettingsID)), 0);
            set => SetValue(nameof(SEOSettingsID), value);
        }


        /// <summary>
        /// Seo settings robots.
        /// </summary>
        [DatabaseField]
        public virtual string SeoSettingsRobots
        {
            get => ValidationHelper.GetString(GetValue(nameof(SeoSettingsRobots)), String.Empty);
            set => SetValue(nameof(SeoSettingsRobots), value, String.Empty);
        }


        /// <summary>
        /// Seo settings web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int SeoSettingsWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SeoSettingsWebChannelSettingID)), 0);
            set => SetValue(nameof(SeoSettingsWebChannelSettingID), value);
        }


        /// <summary>
        /// Seo settings GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid SeoSettingsGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(SeoSettingsGUID)), Guid.Empty);
            set => SetValue(nameof(SeoSettingsGUID), value);
        }


        /// <summary>
        /// Seo settings security.
        /// </summary>
        [DatabaseField]
        public virtual string SeoSettingsSecurity
        {
            get => ValidationHelper.GetString(GetValue(nameof(SeoSettingsSecurity)), String.Empty);
            set => SetValue(nameof(SeoSettingsSecurity), value, String.Empty);
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
        /// Creates an empty instance of the <see cref="SEOSettingsInfo"/> class.
        /// </summary>
        public SEOSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SEOSettingsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SEOSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}