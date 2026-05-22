using BDO;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using GWTXbyKWeb.Controllers;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;


[assembly: RegisterWebPageRoute(HomePage.CONTENT_TYPE_NAME, typeof(HomeController))]

[assembly: RegisterPageTemplate(
    identifier: HomePage.CONTENT_TYPE_NAME,
    name: "Home Page Content Type Template",
    customViewName: "~/Views/Home/Index.cshtml",
    ContentTypeNames = new[] { "BDO.HomePage" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(AboutPage.CONTENT_TYPE_NAME, typeof(AboutPageController))]

[assembly: RegisterPageTemplate(
    identifier: AboutPage.CONTENT_TYPE_NAME,
    name: "About Page Content Type Template",
    customViewName: "~/Views/About/Index.cshtml",
    ContentTypeNames = new[] { "BDO.AboutPage" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(SectionAbout.CONTENT_TYPE_NAME, typeof(SectionAboutController))]

[assembly: RegisterPageTemplate(
    identifier: SectionAbout.CONTENT_TYPE_NAME,
    name: "Section About Content Type Template",
    customViewName: "~/Views/About/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionAbout" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Contact.CONTENT_TYPE_NAME, typeof(ContactController))]

[assembly: RegisterPageTemplate(
    identifier: Contact.CONTENT_TYPE_NAME,
    name: "Contact us Page Content Type Template",
    customViewName: "~/Views/FormPage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Contact" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(SectionIndustries.CONTENT_TYPE_NAME, typeof(SectionIndustriesController))]
[assembly: RegisterWebPageRoute(IndustryCategory.CONTENT_TYPE_NAME, typeof(IndustryCategoryController))]
[assembly: RegisterWebPageRoute(Industry.CONTENT_TYPE_NAME, typeof(IndustryController))]
[assembly: RegisterWebPageRoute(IndustryService.CONTENT_TYPE_NAME, typeof(IndustryServiceController))]


[assembly: RegisterWebPageRoute(SectionServices.CONTENT_TYPE_NAME, typeof(SectionServicesController))]
[assembly: RegisterPageTemplate(
    identifier: SectionServices.CONTENT_TYPE_NAME,
    name: "Section Services Page Content Type Template",
    customViewName: "~/Views/SectionServices/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionServices" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Sub_Service.CONTENT_TYPE_NAME, typeof(SubServiceController))]
[assembly: RegisterPageTemplate(
    identifier: Sub_Service.CONTENT_TYPE_NAME,
    name: "Sub Services Page Content Type Template",
    customViewName: "~/Views/SubServices/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SubServices" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Service.CONTENT_TYPE_NAME, typeof(ServicePageController))]
[assembly: RegisterPageTemplate(
    identifier: Service.CONTENT_TYPE_NAME,
    name: "Services Page Content Type Template",
    customViewName: "~/Views/ServicePage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Service" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(ServiceArea.CONTENT_TYPE_NAME, typeof(ServiceAreaController))]
[assembly: RegisterPageTemplate(
    identifier: ServiceArea.CONTENT_TYPE_NAME,
    name: "Service Area Page Content Type Template",
    customViewName: "~/Views/ServiceArea/Index.cshtml",
    ContentTypeNames = new[] { "BDO.ServiceArea" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(BusinessLine.CONTENT_TYPE_NAME, typeof(BusinessLineController))]
[assembly: RegisterPageTemplate(
    identifier: BusinessLine.CONTENT_TYPE_NAME,
    name: "Business Line Page Content Type Template",
    customViewName: "~/Views/BusinessLine/Index.cshtml",
    ContentTypeNames = new[] { "BDO.BusinessLine" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(SectionPeoplePage.CONTENT_TYPE_NAME, typeof(SectionPeoplePageController))]

[assembly: RegisterPageTemplate(
    identifier: SectionPeoplePage.CONTENT_TYPE_NAME,
    name: "People Page Content Type Template",
    customViewName: "~/Views/SectionPeoplePage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionPeoplePage" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Person.CONTENT_TYPE_NAME, typeof(PersonController))]

[assembly: RegisterPageTemplate(
    identifier: Person.CONTENT_TYPE_NAME,
    name: "Person Page Content Type Template",
    customViewName: "~/Views/Person/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Person" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Locations.CONTENT_TYPE_NAME, typeof(LocationsController))]

[assembly: RegisterPageTemplate(
    identifier: Locations.CONTENT_TYPE_NAME,
    name: "Locations Content Type Template",
    customViewName: "~/Views/Locations/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Locations" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(LocationCity.CONTENT_TYPE_NAME, typeof(LocationCityController))]

[assembly: RegisterPageTemplate(
    identifier: LocationCity.CONTENT_TYPE_NAME,
    name: "Location City Content Type Template",
    customViewName: "~/Views/LocationCity/Index.cshtml",
    ContentTypeNames = new[] { "BDO.LocationCity" },
    IconClass = "xp-market")]

[assembly: RegisterPageTemplate(
    identifier: SectionEvents.CONTENT_TYPE_NAME,
    name: "Section Events Content Type Template",
    customViewName: "~/Views/SectionEvent/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionEvents" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(SectionEvents.CONTENT_TYPE_NAME, typeof(SectionEventsController))]

[assembly: RegisterPageTemplate(
    identifier: EventRegistrationForm.CONTENT_TYPE_NAME,
    name: "Events Registration form Content Type Template",
    customViewName: "~/Views/FormPage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.EventRegistrationForm" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(EventRegistrationForm.CONTENT_TYPE_NAME, typeof(EventRegistrationFormController))]
