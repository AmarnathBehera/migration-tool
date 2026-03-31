using BDO.GWT.Common.Exceptions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Common.Utilities;

public static class HttpResponseUtility
{
    public static async Task EnsureResponseIsSuccessfulAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var url = response.RequestMessage?.RequestUri;

        var bodyString = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new UnsuccessfulResponseException($"Received unsuccessful status code when requesting '{url}' (status: {response.StatusCode}, body: '{bodyString}').");
    }
}
