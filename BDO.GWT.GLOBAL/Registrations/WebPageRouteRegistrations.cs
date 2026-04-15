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

[assembly: RegisterWebPageRoute(Contact.CONTENT_TYPE_NAME, typeof(ContactController))]
[assembly: RegisterPageTemplate(
    identifier: Contact.CONTENT_TYPE_NAME,
    name: "Home Page Content Type Template",
    customViewName: "~/Views/FormPage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Contact" },
    IconClass = "xp-market")]