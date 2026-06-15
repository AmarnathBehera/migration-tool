using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ReleaseinformationInfo), ReleaseinformationInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ReleaseinformationInfo"/>.
    /// </summary>
    public partial class ReleaseinformationInfo : AbstractInfo<ReleaseinformationInfo, IInfoProvider<ReleaseinformationInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.releaseinformation";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ReleaseinformationInfo>), OBJECT_TYPE, "BDO.Releaseinformation", "ReleaseinformationID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Releaseinformation ID.
        /// </summary>
        [DatabaseField]
        public virtual int ReleaseinformationID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ReleaseinformationID)), 0);
            set => SetValue(nameof(ReleaseinformationID), value);
        }


        /// <summary>
        /// .
        /// </summary>
        [DatabaseField]
        public virtual bool maintenancemode
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(maintenancemode)), false);
            set => SetValue(nameof(maintenancemode), value);
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
        /// Creates an empty instance of the <see cref="ReleaseinformationInfo"/> class.
        /// </summary>
        public ReleaseinformationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ReleaseinformationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ReleaseinformationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}