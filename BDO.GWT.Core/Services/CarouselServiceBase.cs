using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public abstract class CarouselServiceBase<TState> where TState : CarouselState, new()
{
    private readonly ICarouselStateHelper<TState> _carouselStateHelper;

    protected CarouselServiceBase(ICarouselStateHelper<TState> carouselStateHelper)
    {
        _carouselStateHelper = carouselStateHelper;
    }

    public async Task<ActionResult<T>> ValidateStateAsync<T>(Guid? state, Func<Task<ActionResult<T>>> onValidAsync)
    {
        if (state is null || !IsValidCarouselState(state.Value))
        {
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }
        
        return await onValidAsync();
    }

    private bool IsValidCarouselState(Guid state)
    {
        return _carouselStateHelper.IsValidCarouselState(state);
    }
}
