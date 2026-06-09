using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CentralRepositoryInfo), CentralRepositoryInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CentralRepositoryInfo"/>.
    /// </summary>
    public partial class CentralRepositoryInfo : AbstractInfo<CentralRepositoryInfo, IInfoProvider<CentralRepositoryInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.centralrepository";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CentralRepositoryInfo>), OBJECT_TYPE, "BDO.CentralRepository", "CentralRepositoryID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Central repository ID.
        /// </summary>
        [DatabaseField]
        public virtual int CentralRepositoryID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CentralRepositoryID)), 0);
            set => SetValue(nameof(CentralRepositoryID), value);
        }


        /// <summary>
        /// Centralrepository instructions.
        /// </summary>
        [DatabaseField]
        public virtual string CentralrepositoryInstructions
        {
            get => ValidationHelper.GetString(GetValue(nameof(CentralrepositoryInstructions)), String.Empty);
            set => SetValue(nameof(CentralrepositoryInstructions), value);
        }


        /// <summary>
        /// Compliancemessage.
        /// </summary>
        [DatabaseField]
        public virtual string Compliancemessage
        {
            get => ValidationHelper.GetString(GetValue(nameof(Compliancemessage)), String.Empty);
            set => SetValue(nameof(Compliancemessage), value);
        }


        /// <summary>
        /// Excludesitefrom central repository.
        /// </summary>
        [DatabaseField]
        public virtual bool ExcludesitefromCentralRepository
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ExcludesitefromCentralRepository)), false);
            set => SetValue(nameof(ExcludesitefromCentralRepository), value);
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
        /// Creates an empty instance of the <see cref="CentralRepositoryInfo"/> class.
        /// </summary>
        public CentralRepositoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CentralRepositoryInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CentralRepositoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}