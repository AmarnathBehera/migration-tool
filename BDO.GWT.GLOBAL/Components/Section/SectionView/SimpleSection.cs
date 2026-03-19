using BDO;
using Kentico.PageBuilder.Web.Mvc;

[assembly: RegisterSection(
    identifier: ComponentIdentifiers.Simple,
    name: "Simple section",
    propertiesType: null,
    customViewName: "~/Components/Section/SectionView/_SimpleSection.cshtml"
)]
public class SimpleSection
{
}
