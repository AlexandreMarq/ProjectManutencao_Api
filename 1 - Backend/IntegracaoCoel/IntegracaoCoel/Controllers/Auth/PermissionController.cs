using AppCoel.Core.Services.Auth;
using AppCoel.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppCoel.Core.API.Controllers.Auth
{
    [Route("api/auth/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PermissionController(IPermissionService permissionService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserPermissionAsync([FromQuery] PermissionContext? context = null, CancellationToken cancellationToken = default )
        {
            var permissions = await permissionService.GetCurrentUserPermissionAsync(context, cancellationToken);
            return this.Ok(permissions);
        }
    }
}
