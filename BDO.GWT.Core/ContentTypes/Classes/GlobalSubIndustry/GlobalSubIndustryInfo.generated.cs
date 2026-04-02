using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalSubIndustryInfo), GlobalSubIndustryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalSubIndustryInfo"/>.
    /// </summary>
    public partial class GlobalSubIndustryInfo : AbstractInfo<GlobalSubIndustryInfo, IInfoProvider<GlobalSubIndustryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.globalsubindustry";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalSubIndustryInfo>), OBJECT_TYPE, "BDODealCards.GlobalSubIndustry", "GlobalSubIndustryID", "GlobalSubIndustryLastModified", "GlobalSubIndustryGuid", "GlobalSubIndustryCodeName", "GlobalSubIndustryName", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Global sub industry ID.
        /// </summary>
        [DatabaseField]
        public virtual int GlobalSubIndustryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GlobalSubIndustryID)), 0);
            set => SetValue(nameof(GlobalSubIndustryID), value);
        }


        /// <summary>
        /// Global sub industry name.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalSubIndustryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalSubIndustryName)), String.Empty);
            set => SetValue(nameof(GlobalSubIndustryName), value);
        }


        /// <summary>
        /// Global sub industry code name.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalSubIndustryCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalSubIndustryCodeName)), String.Empty);
            set => SetValue(nameof(GlobalSubIndustryCodeName), value);
        }


        /// <summary>
        /// Global sub industry related.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalSubIndustryRelated
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalSubIndustryRelated)), String.Empty);
            set => SetValue(nameof(GlobalSubIndustryRelated), value);
        }


        /// <summary>
        /// Global sub industry guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GlobalSubIndustryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(GlobalSubIndustryGuid)), Guid.Empty);
            set => SetValue(nameof(GlobalSubIndustryGuid), value);
        }


        /// <summary>
        /// Global sub industry last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GlobalSubIndustryLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(GlobalSubIndustryLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(GlobalSubIndustryLastModified), value);
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
        /// Creates an empty instance of the <see cref="GlobalSubIndustryInfo"/> class.
        /// </summary>
        public GlobalSubIndustryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalSubIndustryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalSubIndustryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}