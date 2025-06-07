using Microsoft.AspNetCore.Mvc;

namespace GotoCarRental.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        [HttpGet("KeepAlive")]
        public IActionResult KeepAlive()
        {
            // Đơn giản trả về 200 OK để giữ phiên đăng nhập
            return Ok(new { success = true, timestamp = DateTime.Now });
        }
    }
}
