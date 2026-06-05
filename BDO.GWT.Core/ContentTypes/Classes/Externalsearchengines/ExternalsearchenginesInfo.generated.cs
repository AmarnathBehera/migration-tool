using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ExternalsearchenginesInfo), ExternalsearchenginesInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ExternalsearchenginesInfo"/>.
    /// </summary>
    public partial class ExternalsearchenginesInfo : AbstractInfo<ExternalsearchenginesInfo, IInfoProvider<ExternalsearchenginesInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.externalsearchengines";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ExternalsearchenginesInfo>), OBJECT_TYPE, "BDO.Externalsearchengines", "ExternalsearchenginesID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Externalsearchengines ID.
        /// </summary>
        [DatabaseField]
        public virtual int ExternalsearchenginesID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ExternalsearchenginesID)), 0);
            set => SetValue(nameof(ExternalsearchenginesID), value);
        }


        /// <summary>
        /// .
        /// </summary>
        [DatabaseField]
        public virtual bool noindex
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(noindex)), false);
            set => SetValue(nameof(noindex), value);
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
        /// Creates an empty instance of the <see cref="ExternalsearchenginesInfo"/> class.
        /// </summary>
        public ExternalsearchenginesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ExternalsearchenginesInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ExternalsearchenginesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}