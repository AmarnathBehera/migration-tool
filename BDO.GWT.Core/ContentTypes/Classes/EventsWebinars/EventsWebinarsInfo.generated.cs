using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(EventsWebinarsInfo), EventsWebinarsInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="EventsWebinarsInfo"/>.
    /// </summary>
    public partial class EventsWebinarsInfo : AbstractInfo<EventsWebinarsInfo, IInfoProvider<EventsWebinarsInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.eventswebinars";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<EventsWebinarsInfo>), OBJECT_TYPE, "BDO.EventsWebinars", "EventsWebinarsID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("EventnWebinarWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Events webinars ID.
        /// </summary>
        [DatabaseField]
        public virtual int EventsWebinarsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EventsWebinarsID)), 0);
            set => SetValue(nameof(EventsWebinarsID), value);
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
        /// Eventn webinar web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int EventnWebinarWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(EventnWebinarWebChannelSettingID)), 0);
            set => SetValue(nameof(EventnWebinarWebChannelSettingID), value);
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
        /// Creates an empty instance of the <see cref="EventsWebinarsInfo"/> class.
        /// </summary>
        public EventsWebinarsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="EventsWebinarsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public EventsWebinarsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}