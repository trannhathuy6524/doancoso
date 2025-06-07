using GotoCarRental.Services;
using Microsoft.AspNetCore.Mvc;

namespace GotoCarRental.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComparisonAPIController : ControllerBase
    {
        private readonly IComparisonService _comparisonService;

        public ComparisonAPIController(IComparisonService comparisonService)
        {
            _comparisonService = comparisonService;
        }

        [HttpGet]
        public IActionResult GetComparisonList()
        {
            return Ok(_comparisonService.GetComparisonList());
        }

        [HttpPost("add/{carId}")]
        public IActionResult AddToComparison(int carId)
        {
            bool success = _comparisonService.AddToComparison(carId);
            return Ok(new
            {
                success,
                message = success ? "Thêm thành công" : "Không thể thêm vào danh sách so sánh",
                comparisonList = _comparisonService.GetComparisonList()
            });
        }

        [HttpPost("remove/{carId}")]
        public IActionResult RemoveFromComparison(int carId)
        {
            bool success = _comparisonService.RemoveFromComparison(carId);
            return Ok(new
            {
                success,
                message = success ? "Đã xóa khỏi danh sách so sánh" : "Không tìm thấy xe trong danh sách so sánh",
                comparisonList = _comparisonService.GetComparisonList()
            });
        }

        [HttpPost("clear")]
        public IActionResult ClearComparison()
        {
            _comparisonService.ClearComparison();
            return Ok(new { success = true, message = "Đã xóa tất cả xe khỏi danh sách so sánh" });
        }

        [HttpGet("check/{carId}")]
        public IActionResult IsInComparison(int carId)
        {
            return Ok(new { isInComparison = _comparisonService.IsInComparison(carId) });
        }
    }
}
