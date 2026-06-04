#if DEBUG
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
    }
}
#endif