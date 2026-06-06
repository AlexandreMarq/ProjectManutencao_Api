#if DEBUG
using AppCoel.Core.Models.General;
using AppCoel.Exceptions;
using AppCoel.Models;
using AppCoel.Resources;
using Microsoft.AspNetCore.Mvc;

namespace AppCoel.Core.Controllers.General
{
    [Route("api/general/[controller]/[action]")]
    [ApiController]
    public class SampleController : ControllerBase
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
    }
}
#endif