using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Core.API.Bases;

[Authorize]
[ApiController]
public class EntityControllerBase : ControllerBase
{
}
