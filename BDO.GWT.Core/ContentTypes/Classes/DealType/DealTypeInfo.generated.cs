using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(DealTypeInfo), DealTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="DealTypeInfo"/>.
    /// </summary>
    public partial class DealTypeInfo : AbstractInfo<DealTypeInfo, IInfoProvider<DealTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.dealtype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<DealTypeInfo>), OBJECT_TYPE, "BDODealCards.DealType", "DealTypeID", "DealTypeLastModified", "DealTypeGuid", "DealTypeCodeName", "DealTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("DealTypeSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Deal type ID.
        /// </summary>
        [DatabaseField]
        public virtual int DealTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(DealTypeID)), 0);
            set => SetValue(nameof(DealTypeID), value);
        }


        /// <summary>
        /// Deal type name.
        /// </summary>
        [DatabaseField]
        public virtual string DealTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DealTypeName)), String.Empty);
            set => SetValue(nameof(DealTypeName), value);
        }


        /// <summary>
        /// Deal type code name.
        /// </summary>
        [DatabaseField]
        public virtual string DealTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(DealTypeCodeName)), String.Empty);
            set => SetValue(nameof(DealTypeCodeName), value);
        }


        /// <summary>
        /// Deal type culture code.
        /// </summary>
        [DatabaseField]
        public virtual string DealTypeCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(DealTypeCultureCode)), String.Empty);
            set => SetValue(nameof(DealTypeCultureCode), value);
        }


        /// <summary>
        /// Deal type site ID.
        /// </summary>
        [DatabaseField]
        public virtual int DealTypeSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(DealTypeSiteID)), 0);
            set => SetValue(nameof(DealTypeSiteID), value);
        }


        /// <summary>
        /// Deal type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid DealTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(DealTypeGuid)), Guid.Empty);
            set => SetValue(nameof(DealTypeGuid), value);
        }


        /// <summary>
        /// Deal type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DealTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(DealTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(DealTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="DealTypeInfo"/> class.
        /// </summary>
        public DealTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="DealTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public DealTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}