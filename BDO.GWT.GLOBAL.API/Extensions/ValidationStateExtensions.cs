using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Services;

namespace BDO.GWT.GLOBAL.API.Extensions;

public static class ValidationStateExtensions
{
    public static void ThrowIfThereAreErrors(this IValidationState validationState)
    {
        if (validationState.HasErrors())
        {
            throw new ValidationException();
        }

        if (validationState.HasUnprocessableEntityErrors())
        {
            throw new UnprocessableEntityException();
        }
    }
}
