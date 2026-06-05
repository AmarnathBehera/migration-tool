using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ContentDisplaySettingsInfo), ContentDisplaySettingsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ContentDisplaySettingsInfo"/>.
    /// </summary>
    public partial class ContentDisplaySettingsInfo : AbstractInfo<ContentDisplaySettingsInfo, IInfoProvider<ContentDisplaySettingsInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.contentdisplaysettings";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ContentDisplaySettingsInfo>), OBJECT_TYPE, "BDO.ContentDisplaySettings", "ContentDisplaySettingsID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Content display settings ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContentDisplaySettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ContentDisplaySettingsID)), 0);
            set => SetValue(nameof(ContentDisplaySettingsID), value);
        }


        /// <summary>
        /// Underlinelinks.
        /// </summary>
        [DatabaseField]
        public virtual bool Underlinelinks
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Underlinelinks)), false);
            set => SetValue(nameof(Underlinelinks), value);
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
        /// Creates an empty instance of the <see cref="ContentDisplaySettingsInfo"/> class.
        /// </summary>
        public ContentDisplaySettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ContentDisplaySettingsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContentDisplaySettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}