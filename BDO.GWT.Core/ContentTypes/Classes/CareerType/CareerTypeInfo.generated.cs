using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CareerTypeInfo), CareerTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CareerTypeInfo"/>.
    /// </summary>
    public partial class CareerTypeInfo : AbstractInfo<CareerTypeInfo, IInfoProvider<CareerTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.careertype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CareerTypeInfo>), OBJECT_TYPE, "BDOMetadata.CareerType", "CareerTypeID", "CareerTypeLastModified", "CareerTypeGuid", "CareerTypeCodeName", "CareerTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CareerTypeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Career type ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerTypeID)), 0);
            set => SetValue(nameof(CareerTypeID), value);
        }


        /// <summary>
        /// Career type name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerTypeName)), String.Empty);
            set => SetValue(nameof(CareerTypeName), value);
        }


        /// <summary>
        /// Career type code name.
        /// </summary>
        [DatabaseField]
        public virtual string CareerTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerTypeCodeName)), String.Empty);
            set => SetValue(nameof(CareerTypeCodeName), value);
        }


        /// <summary>
        /// Career type culture code.
        /// </summary>
        [DatabaseField]
        public virtual string CareerTypeCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(CareerTypeCultureCode)), String.Empty);
            set => SetValue(nameof(CareerTypeCultureCode), value);
        }


        /// <summary>
        /// Career type site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CareerTypeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CareerTypeSiteID)), 0);
            set => SetValue(nameof(CareerTypeSiteID), value);
        }


        /// <summary>
        /// Career type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid CareerTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(CareerTypeGuid)), Guid.Empty);
            set => SetValue(nameof(CareerTypeGuid), value);
        }


        /// <summary>
        /// Career type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CareerTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(CareerTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(CareerTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="CareerTypeInfo"/> class.
        /// </summary>
        public CareerTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CareerTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CareerTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}