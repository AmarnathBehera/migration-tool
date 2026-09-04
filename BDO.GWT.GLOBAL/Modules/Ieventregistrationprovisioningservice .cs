using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.EventRegistration
{
    public interface IEventRegistrationProvisioningService
    {
        /// <summary>
        /// Creates a child "Event Registration" page with its own dedicated BizForm
        /// under the given Event page, but only if one doesn't already exist
        /// (i.e. effectively "first publish only" - safe to call on every publish).
        /// </summary>
        /// <param name="eventPageId">WebPageItemID of the published Event page.</param>
        /// <param name="eventDisplayName">Display name of the Event page, used to name the registration page and form.</param>
        /// <param name="languageName">Language of the published variant.</param>
        Task EnsureRegistrationPageAsync(
            int eventPageId,
            string eventDisplayName,
            string languageName,
            CancellationToken cancellationToken);
    }
}