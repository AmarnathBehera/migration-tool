using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CareerLevelInfo), CareerLevelInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CareerLevelInfo"/>.
    /// </summary>
    public partial class CareerLevelInfo : AbstractInfo<CareerLevelInfo, IInfoProvider<CareerLevelInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.careerlevel";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CareerLevelInfo>), OBJECT_TYPE, "BDOMetadata.CareerLevel", "CareerLevelID", "CareerLevelLastModified", "CareerLevelGuid", "CareerLevelCodeName", "CareerLevelName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CareerLevelSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Career level ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerLevelID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerLevelID)), 0);
            set => SetValue(nameof(CareerLevelID), value);
        }


        /// <summary>
        /// Career level name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerLevelName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerLevelName)), String.Empty);
            set => SetValue(nameof(CareerLevelName), value);
        }


        /// <summary>
        /// Career level code name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerLevelCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerLevelCodeName)), String.Empty);
            set => SetValue(nameof(CareerLevelCodeName), value);
        }


        /// <summary>
        /// Career level culture code.
        /// </summary>
        [DatabaseField]
        public virtual string CareerLevelCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerLevelCultureCode)), String.Empty);
            set => SetValue(nameof(CareerLevelCultureCode), value);
        }


        /// <summary>
        /// Career level site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerLevelSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerLevelSiteID)), 0);
            set => SetValue(nameof(CareerLevelSiteID), value);
        }


        /// <summary>
        /// Career level guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid CareerLevelGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(CareerLevelGuid)), Guid.Empty);
            set => SetValue(nameof(CareerLevelGuid), value);
        }


        /// <summary>
        /// Career level last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CareerLevelLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CareerLevelLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CareerLevelLastModified), value);
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
        /// Creates an empty instance of the <see cref="CareerLevelInfo"/> class.
        /// </summary>
        public CareerLevelInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CareerLevelInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CareerLevelInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}