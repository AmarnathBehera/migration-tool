using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.LiveSite.Controllers.API;

[Route("[controller]")]
public class VCardController : ControllerBase
{
    private readonly IPersonPageService _personPageService;

    public VCardController(IPersonPageService personPageService)
    {
        _personPageService = personPageService;
    }

    [HttpGet("{personGuid:GUID}")]
    public async Task<IActionResult> DownloadVCard([FromRoute] Guid personGuid, CancellationToken cancellationToken)
    {
        var source = Url.Action(nameof(DownloadVCard), "VCard", personGuid, Request.Scheme);

        var vCard = await _personPageService.GeneratePersonVCardAsync(personGuid, source, cancellationToken);

        if (vCard is null)
        {
            return NotFound();
        }

        var vCardString = VCardHelper.GenerateVCardString(vCard);
        var vCardBinary = Encoding.Default.GetBytes(vCardString);
        return File(vCardBinary, MediaTypeNames.Application.Octet, $"{vCard.FullName}.vcf");
    }
}
