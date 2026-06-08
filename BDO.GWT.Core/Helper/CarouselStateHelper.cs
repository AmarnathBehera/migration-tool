using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Services;
using System.Text.Json;

namespace BDO.GWT.Web.Core.LiveSite.Helpers;

public interface ICarouselStateHelper<TCarouselState> where TCarouselState : CarouselState, new()
{
    TCarouselState GetCarouselState(Guid stateGUID);

    Guid SetCarouselState(TCarouselState state);

    bool IsValidCarouselState(Guid stateGUID);
}

public class CarouselStateHelper<TCarouselState> : ICarouselStateHelper<TCarouselState> where TCarouselState : CarouselState, new()
{
    private readonly ITemporaryStorage _temporaryStorage;

    public CarouselStateHelper(ITemporaryStorage temporaryStorage)
    {
        _temporaryStorage = temporaryStorage;
    }

    public TCarouselState GetCarouselState(Guid stateGUID)
    {
        if (_temporaryStorage.TryGet(stateGUID, out var value))
        {
            var insightCarouselState = JsonSerializer.Deserialize<TCarouselState>((string)value);

            if (insightCarouselState != null)
            {
                return insightCarouselState;
            }
        }

        return new TCarouselState();
    }

    public Guid SetCarouselState(TCarouselState state)
    {
        var json = JsonSerializer.Serialize(state);

        var stateGUID = _temporaryStorage.Set(json);

        return stateGUID;
    }

    public bool IsValidCarouselState(Guid stateGUID)
    {
        return _temporaryStorage.Contains(stateGUID);
    }
}
