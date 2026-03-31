using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CareerJobTitleInfo), CareerJobTitleInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CareerJobTitleInfo"/>.
    /// </summary>
    public partial class CareerJobTitleInfo : AbstractInfo<CareerJobTitleInfo, IInfoProvider<CareerJobTitleInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.careerjobtitle";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CareerJobTitleInfo>), OBJECT_TYPE, "BDOMetadata.CareerJobTitle", "CareerJobTitleID", "CareerJobTitleLastModified", "CareerJobTitleGuid", "CareerJobTitleCodeName", "CareerJobTitleName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CareerJobTitleSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Career job title ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerJobTitleID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerJobTitleID)), 0);
            set => SetValue(nameof(CareerJobTitleID), value);
        }


        /// <summary>
        /// Career job title name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerJobTitleName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerJobTitleName)), String.Empty);
            set => SetValue(nameof(CareerJobTitleName), value);
        }


        /// <summary>
        /// Career job title code name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerJobTitleCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerJobTitleCodeName)), String.Empty);
            set => SetValue(nameof(CareerJobTitleCodeName), value);
        }


        /// <summary>
        /// Career job title culture code.
        /// </summary>
        [DatabaseField]
        public virtual string CareerJobTitleCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerJobTitleCultureCode)), String.Empty);
            set => SetValue(nameof(CareerJobTitleCultureCode), value);
        }


        /// <summary>
        /// Career job title site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerJobTitleSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerJobTitleSiteID)), 0);
            set => SetValue(nameof(CareerJobTitleSiteID), value);
        }


        /// <summary>
        /// Career job title guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid CareerJobTitleGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(CareerJobTitleGuid)), Guid.Empty);
            set => SetValue(nameof(CareerJobTitleGuid), value);
        }


        /// <summary>
        /// Career job title last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CareerJobTitleLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CareerJobTitleLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CareerJobTitleLastModified), value);
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
        /// Creates an empty instance of the <see cref="CareerJobTitleInfo"/> class.
        /// </summary>
        public CareerJobTitleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CareerJobTitleInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CareerJobTitleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}