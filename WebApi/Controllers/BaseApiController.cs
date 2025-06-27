using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        [NonAction]
        public IActionResult Found(object obj)
        {
            return Ok(obj);
        }

        [NonAction]
        public IActionResult Found()
        {
            return Ok();
        }

        [NonAction]
        public IActionResult DoesNotExist()
        {
            return NotFound();
        }

        [NonAction]
        public IActionResult BadRequestMessage(string message)
        {
            return BadRequest(new { error = message });
        }
    }
}