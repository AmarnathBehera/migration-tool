using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(WidgetGovernanceInfo), WidgetGovernanceInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="WidgetGovernanceInfo"/>.
    /// </summary>
    public partial class WidgetGovernanceInfo : AbstractInfo<WidgetGovernanceInfo, IInfoProvider<WidgetGovernanceInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.widgetgovernance";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<WidgetGovernanceInfo>), OBJECT_TYPE, "BDO.WidgetGovernance", "WidgetGovernanceID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Widget governance ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetGovernanceID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(WidgetGovernanceID)), 0);
            set => SetValue(nameof(WidgetGovernanceID), value);
        }


        /// <summary>
        /// Show advanced widgets.
        /// </summary>
        [DatabaseField]
        public virtual bool ShowAdvancedWidgets
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ShowAdvancedWidgets)), false);
            set => SetValue(nameof(ShowAdvancedWidgets), value);
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
        /// Creates an empty instance of the <see cref="WidgetGovernanceInfo"/> class.
        /// </summary>
        public WidgetGovernanceInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="WidgetGovernanceInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public WidgetGovernanceInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}