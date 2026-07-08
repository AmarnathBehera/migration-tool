using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(ArchivedPagesInfo), ArchivedPagesInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="ArchivedPagesInfo"/>.
    /// </summary>
    public partial class ArchivedPagesInfo : AbstractInfo<ArchivedPagesInfo, IInfoProvider<ArchivedPagesInfo>>, IInfoWithId, IInfoWithName
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdo.archivedpages";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ArchivedPagesInfo>), OBJECT_TYPE, "BDO.ArchivedPages", "ArchivedPagesID", null, null, "PageName", null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Archived pages ID.
        /// </summary>
        [DatabaseField]
        public virtual int ArchivedPagesID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ArchivedPagesID)), 0);
            set => SetValue(nameof(ArchivedPagesID), value);
        }


        /// <summary>
        /// Page name.
        /// </summary>
        [DatabaseField]
        public virtual string PageName
        {
            get => ValidationHelper.GetString(GetValue(nameof(PageName)), String.Empty);
            set => SetValue(nameof(PageName), value);
        }


        /// <summary>
        /// Alias path.
        /// </summary>
        [DatabaseField]
        public virtual string AliasPath
        {
            get => ValidationHelper.GetString(GetValue(nameof(AliasPath)), String.Empty);
            set => SetValue(nameof(AliasPath), value);
        }


        /// <summary>
        /// Last modified by.
        /// </summary>
        [DatabaseField]
        public virtual string LastModifiedBy
        {
            get => ValidationHelper.GetString(GetValue(nameof(LastModifiedBy)), String.Empty);
            set => SetValue(nameof(LastModifiedBy), value);
        }


        /// <summary>
        /// Current workflow step.
        /// </summary>
        [DatabaseField]
        public virtual string CurrentWorkflowStep
        {
            get => ValidationHelper.GetString(GetValue(nameof(CurrentWorkflowStep)), String.Empty);
            set => SetValue(nameof(CurrentWorkflowStep), value);
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
        /// Creates an empty instance of the <see cref="ArchivedPagesInfo"/> class.
        /// </summary>
        public ArchivedPagesInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ArchivedPagesInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ArchivedPagesInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}