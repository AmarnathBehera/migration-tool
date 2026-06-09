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

[assembly: RegisterWebPageRoute(Deal.CONTENT_TYPE_NAME, typeof(DealController))]

[assembly: RegisterPageTemplate(
    identifier: Deal.CONTENT_TYPE_NAME,
    name: "Deal Page Content Type Template",
    customViewName: "~/Views/Deal/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Deal" },
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

[assembly: RegisterWebPageRoute(SectionBlogs.CONTENT_TYPE_NAME, typeof(SectionBlogsController))]

[assembly: RegisterPageTemplate(
    identifier: SectionBlogs.CONTENT_TYPE_NAME,
    name: "Section Blogs Content Type Template",
    customViewName: "~/Views/SectionBlogs/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionBlogs" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(BlogPost.CONTENT_TYPE_NAME, typeof(BlogPostController))]

[assembly: RegisterPageTemplate(
    identifier: BlogPost.CONTENT_TYPE_NAME,
    name: "Blog Post Content Type Template",
    customViewName: "~/Views/BlogPost/Index.cshtml",
    ContentTypeNames = new[] { "BDO.BlogPost" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Blog.CONTENT_TYPE_NAME, typeof(BlogsController))]

[assembly: RegisterPageTemplate(
    identifier: Blog.CONTENT_TYPE_NAME,
    name: "Blog Post Content Type Template",
    customViewName: "~/Views/Blogs/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Blog" },
    IconClass = "xp-market")]


[assembly: RegisterWebPageRoute(SectionCareers.CONTENT_TYPE_NAME, typeof(CareerController))]
[assembly: RegisterPageTemplate(
    identifier: SectionCareers.CONTENT_TYPE_NAME,
    name: "Section Career Page Content Type Template",
    customViewName: "~/Views/Career/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionCareers" },
    IconClass = "xp-l-grid-2-2")]

[assembly: RegisterWebPageRoute(Careers.CONTENT_TYPE_NAME, typeof(CareersController))]
[assembly: RegisterPageTemplate(
    identifier: Careers.CONTENT_TYPE_NAME,
    name: "Careers Page Content Type Template",
    customViewName: "~/Views/Careers/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Careers" },
    IconClass = "xp-l-grid-2-2")]

[assembly: RegisterWebPageRoute(Career.CONTENT_TYPE_NAME, typeof(CareerDetailController))]
[assembly: RegisterPageTemplate(
    identifier: Career.CONTENT_TYPE_NAME,
    name: "Career Page Content Type Template",
    customViewName: "~/Views/CareerDetail/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Career" },
    IconClass = "xp-l-grid-2-2")]

[assembly: RegisterWebPageRoute(CareerCategory.CONTENT_TYPE_NAME, typeof(CareerCategoryController))]
[assembly: RegisterPageTemplate(
    identifier: CareerCategory.CONTENT_TYPE_NAME,
    name: "Career Page Content Type Template",
    customViewName: "~/Views/CareerDetail/Index.cshtml",
    ContentTypeNames = new[] { "BDO.CareerCategory" },
    IconClass = "xp-l-grid-2-2")]

[assembly: RegisterWebPageRoute(CareerContent.CONTENT_TYPE_NAME, typeof(CareerContentController))]
[assembly: RegisterPageTemplate(
    identifier: CareerContent.CONTENT_TYPE_NAME,
    name: "Career Content Page Content Type Template",
    customViewName: "~/Views/CareerContent/Index.cshtml",
    ContentTypeNames = new[] { "BDO.CareerContent" },
    IconClass = "xp-l-grid-2-2")]

[assembly: RegisterWebPageRoute(SectionNews.CONTENT_TYPE_NAME, typeof(NewsSectionController))]

[assembly: RegisterPageTemplate(
    identifier: SectionNews.CONTENT_TYPE_NAME,
    name: "News Section Content Type Template",
    customViewName: "~/Views/NewsSection/Index.cshtml",
    ContentTypeNames = new[] { "BDO.NewsSection" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(NewsGroup.CONTENT_TYPE_NAME, typeof(NewsGroupController))]

[assembly: RegisterPageTemplate(
    identifier: NewsGroup.CONTENT_TYPE_NAME,
    name: "News Group Content Type Template",
    customViewName: "~/Views/NewsGroup/Index.cshtml",
    ContentTypeNames = new[] { "BDO.NewsGroup" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(NewsArticle.CONTENT_TYPE_NAME, typeof(NewsArticleController))]

[assembly: RegisterPageTemplate(
    identifier: NewsArticle.CONTENT_TYPE_NAME,
    name: "News Article Content Type Template",
    customViewName: "~/Views/NewsArticle/Index.cshtml",
    ContentTypeNames = new[] { "BDO.NewsArticle" },
    IconClass = "xp-market")]


[assembly: RegisterWebPageRoute(SpecialtiesPage.CONTENT_TYPE_NAME, typeof(SpecialtiesPageController))]

[assembly: RegisterPageTemplate(
    identifier: SpecialtiesPage.CONTENT_TYPE_NAME,
    name: "Specialties Page  Content Type Template",
    customViewName: "~/Views/SpecialtiesPage/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SpecialtiesPage" },
    IconClass = "xp-rectangle-a")]

[assembly: RegisterWebPageRoute(SpecialtiesArea.CONTENT_TYPE_NAME, typeof(SpecialtiesAreaController))]

[assembly: RegisterPageTemplate(
    identifier: SpecialtiesArea.CONTENT_TYPE_NAME,
    name: "Specialties Area  Content Type Template",
    customViewName: "~/Views/SpecialtiesArea/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SpecialtiesArea" },
    IconClass = "xp-doc")]


[assembly: RegisterWebPageRoute(SpecialtiesCategory.CONTENT_TYPE_NAME, typeof(SpecialtiesCategoryController))]

[assembly: RegisterPageTemplate(
    identifier: SpecialtiesCategory.CONTENT_TYPE_NAME,
    name: "Specialties Category  Content Type Template",
    customViewName: "~/Views/SpecialtiesCategory/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SpecialtiesCategory" },
    IconClass = "xp-doc")]

[assembly: RegisterWebPageRoute(SectionSpecialties.CONTENT_TYPE_NAME, typeof(SectionSpecialtiesController))]

[assembly: RegisterPageTemplate(
    identifier: SectionSpecialties.CONTENT_TYPE_NAME,
    name: "Section Specialties Content Type Template",
    customViewName: "~/Views/SectionSpecialties/Index.cshtml",
    ContentTypeNames = new[] { "BDO.SectionSpecialties" },
    IconClass = "xp-doc")]
  

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

[assembly: RegisterPageTemplate(
    identifier: Event.CONTENT_TYPE_NAME,
    name: "Events  Content Type Template",
    customViewName: "~/Views/EventWebinar/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Event" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Event.CONTENT_TYPE_NAME, typeof(EventController))]

[assembly: RegisterPageTemplate(
    identifier: Webinar.CONTENT_TYPE_NAME,
    name: "Webinar  Content Type Template",
    customViewName: "~/Views/EventWebinar/Index.cshtml",
    ContentTypeNames = new[] { "BDO.Webinar" },
    IconClass = "xp-market")]

[assembly: RegisterWebPageRoute(Webinar.CONTENT_TYPE_NAME, typeof(WebinarController))]
