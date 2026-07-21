using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Controllers;

[ApiController]
[Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
public class EditorsPickController : ControllerBase
{
    private readonly IEditorsPickService _editorsPickService;

    public EditorsPickController(IEditorsPickService editorsPickService)
    {
        _editorsPickService = editorsPickService;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<ICollection<EditorsPickCardDto>>> Items(
        [FromQuery] CardsQuery query,
        [FromQuery(Name = "featuredGuids")] string featuredGuids,
        [FromQuery(Name = "selectedGuids")] string selectedGuids,
        CancellationToken cancellationToken)
    {
        query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);

        var featured = ParseContentItemReferences(featuredGuids);
        var selected = ParseContentItemReferences(selectedGuids);

        var result = await _editorsPickService.GetFilteredItemsAsync(
            featured,
            selected,
            query,
            cancellationToken);

        return Ok(result);
    }

    private static HashSet<ContentItemReference> ParseContentItemReferences(string guidsString)
    {
        if (string.IsNullOrWhiteSpace(guidsString))
        {
            return [];
        }

        return guidsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => new ContentItemReference { Identifier = g!.Value })
            .ToHashSet();
    }
}
