using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CareerEmploymentTypeInfo), CareerEmploymentTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CareerEmploymentTypeInfo"/>.
    /// </summary>
    public partial class CareerEmploymentTypeInfo : AbstractInfo<CareerEmploymentTypeInfo, IInfoProvider<CareerEmploymentTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.careeremploymenttype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CareerEmploymentTypeInfo>), OBJECT_TYPE, "BDOMetadata.CareerEmploymentType", "CareerEmploymentTypeID", "CareerEmploymentTypeLastModified", "CareerEmploymentTypeGuid", "CareerEmploymentTypeCodeName", "CareerEmploymentTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CareerEmploymentTypeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Career employment type ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerEmploymentTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerEmploymentTypeID)), 0);
            set => SetValue(nameof(CareerEmploymentTypeID), value);
        }


        /// <summary>
        /// Career employment type name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerEmploymentTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerEmploymentTypeName)), String.Empty);
            set => SetValue(nameof(CareerEmploymentTypeName), value);
        }


        /// <summary>
        /// Career employment type code name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerEmploymentTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerEmploymentTypeCodeName)), String.Empty);
            set => SetValue(nameof(CareerEmploymentTypeCodeName), value);
        }


        /// <summary>
        /// Career employment type culture code.
        /// </summary>
        [DatabaseField]
        public virtual string CareerEmploymentTypeCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerEmploymentTypeCultureCode)), String.Empty);
            set => SetValue(nameof(CareerEmploymentTypeCultureCode), value);
        }


        /// <summary>
        /// Career employment type site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerEmploymentTypeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerEmploymentTypeSiteID)), 0);
            set => SetValue(nameof(CareerEmploymentTypeSiteID), value);
        }


        /// <summary>
        /// Career employment type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid CareerEmploymentTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(CareerEmploymentTypeGuid)), Guid.Empty);
            set => SetValue(nameof(CareerEmploymentTypeGuid), value);
        }


        /// <summary>
        /// Career employment type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CareerEmploymentTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CareerEmploymentTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CareerEmploymentTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="CareerEmploymentTypeInfo"/> class.
        /// </summary>
        public CareerEmploymentTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CareerEmploymentTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CareerEmploymentTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}