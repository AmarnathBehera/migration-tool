using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.Redirects.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(RedirectListing),
    slug: "create",
    uiPageType: typeof(RedirectCreate),
    name: "Create a redirect",
    templateName: TemplateNames.EDIT,
    order: 200)]

namespace XperienceCommunity.Redirects.UIPages;

internal class RedirectCreate : ModelEditPage<RedirectEditModel>
{
    private RedirectEditModel? _model;
    private readonly IInfoProvider<RedirectInfo> _redirectInfoProvider;
    private readonly IPageLinkGenerator _pageLinkGenerator;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    protected override RedirectEditModel Model => _model ??= new RedirectEditModel();

    public RedirectCreate(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IPageLinkGenerator pageLinkGenerator,
        IInfoProvider<RedirectInfo> RedirectInfoProvider,
        IWebPageUrlRetriever webPageUrlRetriever)
        : base(formItemCollectionProvider, formDataBinder)
    {
        _pageLinkGenerator = pageLinkGenerator;
        _redirectInfoProvider = RedirectInfoProvider;
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    protected override async Task<ICommandResponse> ProcessFormData(RedirectEditModel model, ICollection<IFormItem> formItems)
    {
        await CreateCodeSnippetInfo(model);

        var navigateResponse = await NavigateToEditPage(model, formItems);

        return navigateResponse;
    }

    private async Task<INavigateResponse> NavigateToEditPage(RedirectEditModel model, ICollection<IFormItem> formItems)
    {
        var baseResult = await base.ProcessFormData(model, formItems);

        var navigateResponse = NavigateTo(_pageLinkGenerator.GetPath<RedirectListing>());

        foreach (var message in baseResult.Messages)
        {
            navigateResponse.Messages.Add(message);
        }

        return navigateResponse;
    }

    private async Task CreateCodeSnippetInfo(RedirectEditModel model)
    {
        var infoObject = new RedirectInfo();

        await model.MapToRedirectInfo(infoObject, _webPageUrlRetriever);

        _redirectInfoProvider.Set(infoObject);
    }
}