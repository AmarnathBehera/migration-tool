using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Bases;

[Authorize]
[ApiController]
public class EntityControllerBase : ControllerBase
{
}
