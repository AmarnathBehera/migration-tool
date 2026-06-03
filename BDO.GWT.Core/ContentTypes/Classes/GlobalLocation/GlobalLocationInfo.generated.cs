using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalLocationInfo), GlobalLocationInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalLocationInfo"/>.
    /// </summary>
    public partial class GlobalLocationInfo : AbstractInfo<GlobalLocationInfo, IInfoProvider<GlobalLocationInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.globallocation";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalLocationInfo>), OBJECT_TYPE, "BDO.GlobalLocation", "GlobalLocationID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
            },
        };


        /// <summary>
        /// Global location ID.
        /// </summary>
        [DatabaseField]
        public virtual int GlobalLocationID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GlobalLocationID)), 0);
            set => SetValue(nameof(GlobalLocationID), value);
        }


        /// <summary>
        /// Global locations title.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationsTitle
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationsTitle)), String.Empty);
            set => SetValue(nameof(GlobalLocationsTitle), value);
        }


        /// <summary>
        /// Global locations description.
        /// </summary>
        [DatabaseField]
        public virtual string GlobalLocationsDescription
        {
            get => ValidationHelper.GetString(GetValue(nameof(GlobalLocationsDescription)), String.Empty);
            set => SetValue(nameof(GlobalLocationsDescription), value);
        }


        /// <summary>
        /// View global locations background image.
        /// </summary>
        [DatabaseField]
        [Obsolete("Property is deprecated and will be removed in the future. Use property ViewGlobalLocationsBackgroundImage instead.")]
        public virtual string mViewGlobalLocationsBackgroundImage
        {
            get => ValidationHelper.GetString(GetValue(nameof(ViewGlobalLocationsBackgroundImage)), String.Empty);
            set => SetValue(nameof(ViewGlobalLocationsBackgroundImage), value);
        }


        /// <summary>
        /// View global locations background image.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        [Obsolete("Media libraries and their APIs are obsolete and will be discontinued in the future. Please migrate all media library files to Content hub and use their corresponding content item APIs.")]
        public IEnumerable<global::CMS.MediaLibrary.AssetRelatedItem> ViewGlobalLocationsBackgroundImage
        {
            get => global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToModels<global::CMS.MediaLibrary.AssetRelatedItem>(GetValue(nameof(ViewGlobalLocationsBackgroundImage), String.Empty));
            set => SetValue(nameof(ViewGlobalLocationsBackgroundImage), global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToString<IEnumerable<global::CMS.MediaLibrary.AssetRelatedItem>>(value, Enumerable.Empty<global::CMS.MediaLibrary.AssetRelatedItem>(), null));
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
        /// Creates an empty instance of the <see cref="GlobalLocationInfo"/> class.
        /// </summary>
        public GlobalLocationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalLocationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalLocationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}