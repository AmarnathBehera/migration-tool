

using BDO.GWT.Web.Core.DTOs.Pages;

namespace BDO.GWT.Web.Core.Services;

public interface IContactPageService
{
    Task<FormPageDto> GetContactPageDtoAsync(Contact page, CancellationToken cancellationToken);
}

public class ContactPageService : IContactPageService
{
    private readonly IHeroService _heroService;

    public ContactPageService(IHeroService heroService)
    {
        _heroService = heroService;
    }

    public async Task<FormPageDto> GetContactPageDtoAsync(Contact page, CancellationToken cancellationToken)
    {
        return new FormPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(page, cancellationToken),
            Title = page.DescriptionTitle,
            Description = page.DescriptionBody,
            FormType = page.FormType,
            Form = page.Form,
            EmbedCode = page.EmbedCode,
        };
    }
}
