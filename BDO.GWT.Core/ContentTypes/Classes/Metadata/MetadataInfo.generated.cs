using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(MetadataInfo), MetadataInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="MetadataInfo"/>.
    /// </summary>
    public partial class MetadataInfo : AbstractInfo<MetadataInfo, IInfoProvider<MetadataInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.metadata";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<MetadataInfo>), OBJECT_TYPE, "BDO.Metadata", "MetadataID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Metadata ID.
        /// </summary>
        [DatabaseField]
        public virtual int MetadataID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(MetadataID)), 0);
            set => SetValue(nameof(MetadataID), value);
        }


        /// <summary>
        /// Enablemetadataprocessing.
        /// </summary>
        [DatabaseField]
        public virtual bool Enablemetadataprocessing
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Enablemetadataprocessing)), false);
            set => SetValue(nameof(Enablemetadataprocessing), value);
        }


        /// <summary>
        /// Metadatadescriptionlength.
        /// </summary>
        [DatabaseField]
        public virtual int Metadatadescriptionlength
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(Metadatadescriptionlength)), 0);
            set => SetValue(nameof(Metadatadescriptionlength), value);
        }


        /// <summary>
        /// Trimdescriptiontoonesentence.
        /// </summary>
        [DatabaseField]
        public virtual bool Trimdescriptiontoonesentence
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Trimdescriptiontoonesentence)), false);
            set => SetValue(nameof(Trimdescriptiontoonesentence), value);
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
        /// Creates an empty instance of the <see cref="MetadataInfo"/> class.
        /// </summary>
        public MetadataInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="MetadataInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public MetadataInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}