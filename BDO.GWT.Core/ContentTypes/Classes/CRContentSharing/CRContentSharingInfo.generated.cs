using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(CRContentSharingInfo), CRContentSharingInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="CRContentSharingInfo"/>.
    /// </summary>
    public partial class CRContentSharingInfo : AbstractInfo<CRContentSharingInfo, IInfoProvider<CRContentSharingInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.crcontentsharing";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<CRContentSharingInfo>), OBJECT_TYPE, "BDO.CRContentSharing", "CRContentSharingID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// CR content sharing ID.
        /// </summary>
        [DatabaseField]
        public virtual int CRContentSharingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(CRContentSharingID)), 0);
            set => SetValue(nameof(CRContentSharingID), value);
        }


        /// <summary>
        /// Default insight instructions.
        /// </summary>
        [DatabaseField]
        public virtual string DefaultInsightInstructions
        {
            get => ValidationHelper.GetString(GetValue(nameof(DefaultInsightInstructions)), String.Empty);
            set => SetValue(nameof(DefaultInsightInstructions), value);
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
        /// Creates an empty instance of the <see cref="CRContentSharingInfo"/> class.
        /// </summary>
        public CRContentSharingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="CRContentSharingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public CRContentSharingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}