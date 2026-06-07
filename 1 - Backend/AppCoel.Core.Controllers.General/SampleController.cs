#if DEBUG
using AppCoel.Core.Contracts;
using AppCoel.Core.Models.General;
using AppCoel.Exceptions;
using AppCoel.Models;
using AppCoel.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppCoel.Core.Controllers.General
{
    [Route("api/general/[controller]/[action]")]
    [ApiController]
    public class SampleController(IUserContext userContext, ILogger<SampleController> logger) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSample()
        {
            return this.Ok("This is a sample responce from GeneralSampleController.");
        }

        [HttpGet]
        public IActionResult GetCultureSample()
        {
            return this.Ok(StringResource.GetStringByKey("Sample_Message_Text"));
        }

        [HttpGet]
        public IActionResult GetException()
        {
            throw new AppException(ExceptionCode.Generic, "This is a  sample exception for testing purposes.");
        }

        [HttpPost]
        public IActionResult PostSample([FromBody] CreateOrUpdateRequest<SampleDto> request)
        {
            return this.Ok();
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetProtectedData()
        {
            return this.Ok("This is a protected data.");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCurrentEmailAsync(CancellationToken cancellationToken = default)
        {
            var currentUser = await userContext.GetCurrentUserAsync(cancellationToken);
            var systemAdminUser = await userContext.GetSystemAdminUserAsync(cancellationToken);

            var message = $"Current user email: {currentUser.Email} " + $"System user email: {systemAdminUser.Email}";

            return this.Ok(message);
        }

        [HttpGet]
        public string GetLogSample()
        {
            var logMassage = "This is a sample log message";
            logger.LogInformation(logMassage);

            return logMassage;
        }
    }
}
#endif