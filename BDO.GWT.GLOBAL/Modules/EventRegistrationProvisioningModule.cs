//using BDO.GWT.Xperience.EventRegistration;
//using CMS;
//using CMS.Base.Internal;
//using CMS.Core;
//using CMS.DataEngine;
//using Microsoft.Extensions.DependencyInjection;

//[assembly: RegisterModule(typeof(EventRegistrationProvisioningModule))]

//namespace BDO.GWT.Xperience.EventRegistration
//{
//    /// <summary>
//    /// Registers the event handler and service responsible for auto-provisioning
//    /// an Event Registration page + BizForm the first time an Event page is published.
//    /// </summary>
//    public class EventRegistrationProvisioningModule : Module
//    {
//        public EventRegistrationProvisioningModule() : base(nameof(EventRegistrationProvisioningModule))
//        {
//        }

//        protected override void OnPreInit(ModulePreInitParameters parameters)
//        {
//            base.OnPreInit(parameters);

//            // Registers the DI service used by the event handler
//            parameters.Services.AddSingleton<IEventRegistrationProvisioningService, EventRegistrationProvisioningService>();

//            // Registers the content event handler for the 'AfterPublishWebPageEvent'
//            parameters.Services.AddEventHandler<CMS.Websites.AfterPublishWebPageEvent, EventPublishHandler>();
//        }
//    }
//}