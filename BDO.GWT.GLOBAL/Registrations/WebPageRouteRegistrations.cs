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
    name: "Contact us Page Content Type Template",
    customViewName: "~/Views/FormPage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Contact" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(SectionIndustries.CONTENT_TYPE_NAME, typeof(SectionIndustriesController))]
[assembly: RegisterWebPageRoute(IndustryCategory.CONTENT_TYPE_NAME, typeof(IndustryCategoryController))]
[assembly: RegisterWebPageRoute(Industry.CONTENT_TYPE_NAME, typeof(IndustryController))]
//[assembly: RegisterWebPageRoute(IndustryService.CONTENT_TYPE_NAME, typeof(IndustryServiceController))]
[assembly: RegisterPageTemplate(
    identifier: SectionIndustries.CONTENT_TYPE_NAME,
    name: "Section Industries Page Content Type Template",
    customViewName: "~/Views/SectionIndustries/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionIndustries" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(SectionServices.CONTENT_TYPE_NAME, typeof(SectionServicesController))]
[assembly: RegisterPageTemplate(
    identifier: SectionServices.CONTENT_TYPE_NAME,
    name: "Section Services Page Content Type Template",
    customViewName: "~/Views/SectionServices/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionServices" },
    IconClass = "xp-market")]