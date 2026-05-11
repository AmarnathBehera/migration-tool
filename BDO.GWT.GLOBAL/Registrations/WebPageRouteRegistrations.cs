using BDO;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.LiveSite.Controllers;
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
    name: "Contact Content Type Template",
    customViewName: "~/Views/FormPage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Contact" },
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
