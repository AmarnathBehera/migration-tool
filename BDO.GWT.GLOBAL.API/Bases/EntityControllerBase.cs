using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Bases;

/// <summary>
/// Base class for entity controllers providing common functionality for entity-based API endpoints.
/// </summary>
[Authorize]
[ApiController]
public class EntityControllerBase : ControllerBase
{
}
