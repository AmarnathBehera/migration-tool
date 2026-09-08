using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(GlobalEventsnWebinarsInfo), GlobalEventsnWebinarsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="GlobalEventsnWebinarsInfo"/>.
    /// </summary>
    public partial class GlobalEventsnWebinarsInfo : AbstractInfo<GlobalEventsnWebinarsInfo, IInfoProvider<GlobalEventsnWebinarsInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.globaleventsnwebinars";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<GlobalEventsnWebinarsInfo>), OBJECT_TYPE, "BDO.GlobalEventsnWebinars", "GlobalEventsnWebinarsID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Global eventsn webinars ID.
        /// </summary>
        [DatabaseField]
        public virtual int GlobalEventsnWebinarsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(GlobalEventsnWebinarsID)), 0);
            set => SetValue(nameof(GlobalEventsnWebinarsID), value);
        }


        /// <summary>
        /// Minutestotriggercalendarreminder.
        /// </summary>
        [DatabaseField]
        public virtual int Minutestotriggercalendarreminder
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(Minutestotriggercalendarreminder)), 0);
            set => SetValue(nameof(Minutestotriggercalendarreminder), value);
        }


        /// <summary>
        /// Displaylocationtitleinasidebar.
        /// </summary>
        [DatabaseField]
        public virtual bool Displaylocationtitleinasidebar
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(Displaylocationtitleinasidebar)), false);
            set => SetValue(nameof(Displaylocationtitleinasidebar), value);
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
        /// Creates an empty instance of the <see cref="GlobalEventsnWebinarsInfo"/> class.
        /// </summary>
        public GlobalEventsnWebinarsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="GlobalEventsnWebinarsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public GlobalEventsnWebinarsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}