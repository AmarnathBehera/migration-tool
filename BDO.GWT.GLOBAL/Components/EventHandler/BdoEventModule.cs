// File: Components/EventHandler/BdoEventWorkflowModule.cs

using AngleSharp.Dom;
using BDO.GWT.GLOBAL.Components.EventHandler;
using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;

[assembly: RegisterModule(typeof(BdoEventWorkflowModule))]

namespace BDO.GWT.GLOBAL.Components.EventHandler
{
    public class BdoEventWorkflowModule : Module
    {
        public BdoEventWorkflowModule() : base(nameof(BdoEventWorkflowModule))
        {
        }

        protected override void OnPreInit(ModulePreInitParameters parameters)
        {
            base.OnPreInit(parameters);
            parameters.Services.AddEventHandler<BeforeCreateWebPageEvent, NonAsciiUrlSlugHandler>();
            parameters.Services.AddEventHandler<AfterPublishWebPageEvent, BdoEventPublishHandler>();
        }
    }
}