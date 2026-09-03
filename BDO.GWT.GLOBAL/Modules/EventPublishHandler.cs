//using CMS.Base;
//using CMS.Base.Internal;
//using CMS.Websites;
//using CMS.Websites.Internal;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Xperience.EventRegistration
//{
//    /// <summary>
//    /// Handles the 'AfterPublishWebPageEvent' for pages of the Event content type.
//    /// Delegates the actual provisioning work (and the first-time-only check)
//    /// to <see cref="IEventRegistrationProvisioningService"/>.
//    /// </summary>
//    public class EventPublishHandler : IAsyncEventHandler<AfterPublishWebPageEvent>
//    {
//        // TODO: replace with your actual Event content type code name
//        private const string EVENT_CONTENT_TYPE = "BDO.Event";

//        private readonly IEventRegistrationProvisioningService provisioningService;

//        public EventPublishHandler(IEventRegistrationProvisioningService provisioningService)
//        {
//            this.provisioningService = provisioningService;
//        }

//        public async Task HandleAsync(AfterPublishWebPageEvent asyncEvent, CancellationToken cancellationToken)
//        {
//            PublishWebPageEventData data = asyncEvent.Data;

//            // Only react to the Event content type - ignore every other page publish
//            if (!string.Equals(data.ContentTypeName, EVENT_CONTENT_TYPE, StringComparison.OrdinalIgnoreCase))
//            {
//                return;
//            }

//            await provisioningService.EnsureRegistrationPageAsync(
//                data.ID,
//                data.DisplayName,
//                data.LanguageName,
//                cancellationToken);
//        }
//    }
//}