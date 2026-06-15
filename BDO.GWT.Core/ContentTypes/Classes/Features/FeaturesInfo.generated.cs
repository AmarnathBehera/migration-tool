using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(FeaturesInfo), FeaturesInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="FeaturesInfo"/>.
    /// </summary>
    public partial class FeaturesInfo : AbstractInfo<FeaturesInfo, IInfoProvider<FeaturesInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.features";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<FeaturesInfo>), OBJECT_TYPE, "BDO.Features", "FeaturesID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Features ID.
        /// </summary>
        [DatabaseField]
        public virtual int FeaturesID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(FeaturesID)), 0);
            set => SetValue(nameof(FeaturesID), value);
        }


        /// <summary>
        /// BDO global.
        /// </summary>
        [DatabaseField]
        public virtual bool BDOGlobal
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(BDOGlobal)), false);
            set => SetValue(nameof(BDOGlobal), value);
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
        /// Creates an empty instance of the <see cref="FeaturesInfo"/> class.
        /// </summary>
        public FeaturesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FeaturesInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FeaturesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}