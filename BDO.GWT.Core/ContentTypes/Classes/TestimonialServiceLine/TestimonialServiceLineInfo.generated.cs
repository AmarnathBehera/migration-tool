using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using BDO;

[assembly: RegisterObjectType(typeof(TestimonialServiceLineInfo), TestimonialServiceLineInfo.OBJECT_TYPE)]

namespace BDO
{
    /// <summary>
    /// Data container class for <see cref="TestimonialServiceLineInfo"/>.
    /// </summary>
    public partial class TestimonialServiceLineInfo : AbstractInfo<TestimonialServiceLineInfo, IInfoProvider<TestimonialServiceLineInfo>>, IInfoWithId, IInfoWithName, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "bdometadata.testimonialserviceline";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<TestimonialServiceLineInfo>), OBJECT_TYPE, "BDOMetadata.TestimonialServiceLine", "TestimonialServiceLineID", "TestimonialServiceLineLastModified", "TestimonialServiceLineGuid", "ServiceLineCodeName", "ServiceLineName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("TestimonialServiceLineSiteID", "cms.site", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Testimonial service line ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestimonialServiceLineID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(TestimonialServiceLineID)), 0);
            set => SetValue(nameof(TestimonialServiceLineID), value);
        }


        /// <summary>
        /// Service line name.
        /// </summary>
        [DatabaseField]
        public virtual string ServiceLineName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ServiceLineName)), String.Empty);
            set => SetValue(nameof(ServiceLineName), value);
        }


        /// <summary>
        /// Service line code name.
        /// </summary>
        [DatabaseField]
        public virtual string ServiceLineCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ServiceLineCodeName)), String.Empty);
            set => SetValue(nameof(ServiceLineCodeName), value);
        }


        /// <summary>
        /// Service line culture code.
        /// </summary>
        [DatabaseField]
        public virtual string ServiceLineCultureCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(ServiceLineCultureCode)), String.Empty);
            set => SetValue(nameof(ServiceLineCultureCode), value);
        }


        /// <summary>
        /// Testimonial service line site ID.
        /// </summary>
        [DatabaseField]
        public virtual int TestimonialServiceLineSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(TestimonialServiceLineSiteID)), 0);
            set => SetValue(nameof(TestimonialServiceLineSiteID), value);
        }


        /// <summary>
        /// Testimonial service line guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid TestimonialServiceLineGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(TestimonialServiceLineGuid)), Guid.Empty);
            set => SetValue(nameof(TestimonialServiceLineGuid), value);
        }


        /// <summary>
        /// Testimonial service line last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TestimonialServiceLineLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(TestimonialServiceLineLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(TestimonialServiceLineLastModified), value);
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
        /// Creates an empty instance of the <see cref="TestimonialServiceLineInfo"/> class.
        /// </summary>
        public TestimonialServiceLineInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="TestimonialServiceLineInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TestimonialServiceLineInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}