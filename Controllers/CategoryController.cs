using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GotoCarRental.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICarRepository _carRepository;

        public CategoryController(ICategoryRepository categoryRepository, ICarRepository carRepository)
        {
            _categoryRepository = categoryRepository;
            _carRepository = carRepository;
        }

        // GET: /Category
        public async Task<IActionResult> Index(string searchTerm = null)
        {
            IEnumerable<Category> categories;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                categories = await _categoryRepository.SearchAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                categories = await _categoryRepository.GetAllAsync();
            }

            // Lấy số lượng xe cho mỗi loại xe
            var categoryList = categories.ToList();
            var categoryStats = new Dictionary<int, int>();

            foreach (var category in categoryList)
            {
                categoryStats[category.Id] = await _categoryRepository.GetCarCountAsync(category.Id);
            }

            ViewBag.CategoryStats = categoryStats;

            return View(categoryList);
        }

        // GET: /Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe thuộc loại này
            var cars = await _categoryRepository.GetCarsByCategoryAsync(id);
            ViewBag.Cars = cars;
            ViewBag.CarCount = cars.Count();

            return View(category);
        }

        // GET: /Category/Create
        public IActionResult Create()
        {
            var category = new Category
            {
                Cars = new List<Car>() // Khởi tạo collection rỗng
            };
            return View(category);
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên loại xe có trùng không
                if (await _categoryRepository.ExistsByNameAsync(category.Name))
                {
                    ModelState.AddModelError("Name", "Tên loại xe này đã tồn tại.");
                    return View(category);
                }

                await _categoryRepository.AddAsync(category);
                TempData["SuccessMessage"] = "Thêm loại xe mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: /Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra tên loại xe có trùng không (không tính chính nó)
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory.Name != category.Name && await _categoryRepository.ExistsByNameAsync(category.Name))
                {
                    ModelState.AddModelError("Name", "Tên loại xe này đã tồn tại.");
                    return View(category);
                }

                try
                {
                    await _categoryRepository.UpdateAsync(category);
                    TempData["SuccessMessage"] = "Cập nhật loại xe thành công!";
                }
                catch
                {
                    if (!await _categoryRepository.ExistsAsync(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem loại xe có xe không
            var carCount = await _categoryRepository.GetCarCountAsync(id);
            ViewBag.CarCount = carCount;
            ViewBag.HasCars = carCount > 0;

            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem loại xe có xe không, nếu có thì không cho xóa
            var carCount = await _categoryRepository.GetCarCountAsync(id);
            if (carCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại xe này vì có xe thuộc loại đang tồn tại.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _categoryRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa loại xe thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Category/Cars/5
        public async Task<IActionResult> Cars(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var cars = await _categoryRepository.GetCarsByCategoryAsync(id);
            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = category.Id;

            return View(cars);
        }

        // GET: /Category/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.SearchAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;

            // Lấy số lượng xe cho mỗi loại xe
            var categoryList = categories.ToList();
            var categoryStats = new Dictionary<int, int>();

            foreach (var category in categoryList)
            {
                categoryStats[category.Id] = await _categoryRepository.GetCarCountAsync(category.Id);
            }

            ViewBag.CategoryStats = categoryStats;

            return View("Index", categoryList);
        }
    }
}
