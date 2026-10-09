using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ImageresizingInfo), ImageresizingInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ImageresizingInfo"/>.
    /// </summary>
    public partial class ImageresizingInfo : AbstractInfo<ImageresizingInfo, IInfoProvider<ImageresizingInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.imageresizing";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ImageresizingInfo>), OBJECT_TYPE, "BDO.Imageresizing", "ImageresizingID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Imageresizing ID.
        /// </summary>
        [DatabaseField]
        public virtual int ImageresizingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ImageresizingID)), 0);
            set => SetValue(nameof(ImageresizingID), value);
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
        /// Creates an empty instance of the <see cref="ImageresizingInfo"/> class.
        /// </summary>
        public ImageresizingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ImageresizingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ImageresizingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}