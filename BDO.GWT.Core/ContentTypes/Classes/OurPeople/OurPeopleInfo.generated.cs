using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(OurPeopleInfo), OurPeopleInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="OurPeopleInfo"/>.
    /// </summary>
    public partial class OurPeopleInfo : AbstractInfo<OurPeopleInfo, IInfoProvider<OurPeopleInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.ourpeople";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<OurPeopleInfo>), OBJECT_TYPE, "BDO.OurPeople", "OurPeopleID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("PeopleSettingsWebChannelSettingID", "bdo.webchannelsettings", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Our people ID.
        /// </summary>
        [DatabaseField]
        public virtual int OurPeopleID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(OurPeopleID)), 0);
            set => SetValue(nameof(OurPeopleID), value);
        }


        /// <summary>
        /// Display emailinv card.
        /// </summary>
        [DatabaseField]
        public virtual bool DisplayEmailinvCard
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(DisplayEmailinvCard)), false);
            set => SetValue(nameof(DisplayEmailinvCard), value);
        }


        /// <summary>
        /// Use page title to display person name.
        /// </summary>
        [DatabaseField]
        public virtual bool UsePageTitleToDisplayPersonName
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(UsePageTitleToDisplayPersonName)), false);
            set => SetValue(nameof(UsePageTitleToDisplayPersonName), value);
        }


        /// <summary>
        /// People settings web channel setting ID.
        /// </summary>
        [DatabaseField]
        public virtual int PeopleSettingsWebChannelSettingID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(PeopleSettingsWebChannelSettingID)), 0);
            set => SetValue(nameof(PeopleSettingsWebChannelSettingID), value);
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
        /// Creates an empty instance of the <see cref="OurPeopleInfo"/> class.
        /// </summary>
        public OurPeopleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="OurPeopleInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public OurPeopleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}