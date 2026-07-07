using BDO.GWT.Common.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Common.Core.Services;

public interface IValidationState
{
    void AddError(string key, string value);

    void AddUnprocessableEntityError(string key, string value);

    bool HasErrors();

    bool HasUnprocessableEntityErrors();

    IDictionary<string, string> GetErrors();

    void Clear();
}

public class ValidationState : IValidationState
{
    private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _unprocessableEntityErrors = new Dictionary<string, string>();

    public void AddError(string key, string value)
    {
        _errors[key] = value;
    }

    /// <summary>
    /// Adds an error to the collection of unprocessable entity errors. This method is used to store errors that are related to the processing of a request, but do not necessarily indicate a failure in the request itself. These errors can be used to provide additional information to the client about why the request could not be processed successfully.
    /// </summary>
    /// <param name="key">The key associated with the error.</param>
    /// <param name="value">The error message.</param>
    public void AddUnprocessableEntityError(string key, string value)
    {
        _unprocessableEntityErrors[key] = value;
    }

    public bool HasErrors()
    {
        return _errors.Count > 0;
    }

    public bool HasUnprocessableEntityErrors()
    {
        return _unprocessableEntityErrors.Count > 0;
    }

    public IDictionary<string, string> GetErrors()
    {
        var allErrors = _errors.Concat(_unprocessableEntityErrors);

        return allErrors.ToDictionaryOverwriteExisting(x => FormatKey(x.Key), x => x.Value);
    }

    private static string FormatKey(string key)
    {
        return $"{char.ToLowerInvariant(key[0])}{key[1..]}";
    }

    public void Clear()
    {
        _errors.Clear();
        _unprocessableEntityErrors.Clear();
    }
}
