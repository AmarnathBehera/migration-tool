using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalIndustryInfo), GlobalIndustryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalIndustryInfo"/>.
    /// </summary>
    public partial class GlobalIndustryInfo : AbstractInfo<GlobalIndustryInfo, IInfoProvider<GlobalIndustryInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.globalindustry";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalIndustryInfo>), OBJECT_TYPE, "BDODealCards.GlobalIndustry", "GlobalIndustryID", "GlobalIndustryLastModified", "GlobalIndustryGuid", "GlobalIndustryCodeName", "GlobalIndustryName", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Global industry ID.
        /// </summary>
        [DatabaseField]
        public virtual int GlobalIndustryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GlobalIndustryID)), 0);
            set => SetValue(nameof(GlobalIndustryID), value);
        }


        /// <summary>
        /// Global industry name.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalIndustryName
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalIndustryName)), String.Empty);
            set => SetValue(nameof(GlobalIndustryName), value);
        }


        /// <summary>
        /// Global industry code name.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalIndustryCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalIndustryCodeName)), String.Empty);
            set => SetValue(nameof(GlobalIndustryCodeName), value);
        }


        /// <summary>
        /// Global industry guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GlobalIndustryGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(GlobalIndustryGuid)), Guid.Empty);
            set => SetValue(nameof(GlobalIndustryGuid), value);
        }


        /// <summary>
        /// Global industry last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GlobalIndustryLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(GlobalIndustryLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(GlobalIndustryLastModified), value);
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
        /// Creates an empty instance of the <see cref="GlobalIndustryInfo"/> class.
        /// </summary>
        public GlobalIndustryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalIndustryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalIndustryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}