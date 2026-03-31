using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalDealTypeInfo), GlobalDealTypeInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalDealTypeInfo"/>.
    /// </summary>
    public partial class GlobalDealTypeInfo : AbstractInfo<GlobalDealTypeInfo, IInfoProvider<GlobalDealTypeInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdodealcards.globaldealtype";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalDealTypeInfo>), OBJECT_TYPE, "BDODealCards.GlobalDealType", "GlobalDealTypeID", "GlobalDealTypeLastModified", "GlobalDealTypeGuid", "GlobalDealTypeCodeName", "GlobalDealTypeName", null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Global deal type ID.
        /// </summary>
        [DatabaseField]
        public virtual int GlobalDealTypeID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GlobalDealTypeID)), 0);
            set => SetValue(nameof(GlobalDealTypeID), value);
        }


        /// <summary>
        /// Global deal type name.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalDealTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalDealTypeName)), String.Empty);
            set => SetValue(nameof(GlobalDealTypeName), value);
        }


        /// <summary>
        /// Global deal type code name.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalDealTypeCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalDealTypeCodeName)), String.Empty);
            set => SetValue(nameof(GlobalDealTypeCodeName), value);
        }


        /// <summary>
        /// Global deal type guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid GlobalDealTypeGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(GlobalDealTypeGuid)), Guid.Empty);
            set => SetValue(nameof(GlobalDealTypeGuid), value);
        }


        /// <summary>
        /// Global deal type last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime GlobalDealTypeLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(GlobalDealTypeLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(GlobalDealTypeLastModified), value);
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
        /// Creates an empty instance of the <see cref="GlobalDealTypeInfo"/> class.
        /// </summary>
        public GlobalDealTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalDealTypeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalDealTypeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}