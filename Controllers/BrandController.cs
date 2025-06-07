using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GotoCarRental.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ICarRepository _carRepository;

        public BrandController(IBrandRepository brandRepository, ICarRepository carRepository)
        {
            _brandRepository = brandRepository;
            _carRepository = carRepository;
        }

        // GET: /Brand
        public async Task<IActionResult> Index(string searchTerm = null)
        {
            IEnumerable<Brand> brands;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                brands = await _brandRepository.SearchAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                brands = await _brandRepository.GetAllAsync();
            }

            // Lấy số lượng xe cho mỗi hãng
            var brandList = brands.ToList();
            var brandStats = new Dictionary<int, int>();

            foreach (var brand in brandList)
            {
                brandStats[brand.Id] = await _brandRepository.GetCarCountAsync(brand.Id);
            }

            ViewBag.BrandStats = brandStats;

            return View(brandList);
        }

        // GET: /Brand/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe thuộc hãng này
            var cars = await _brandRepository.GetCarsByBrandAsync(id);
            ViewBag.Cars = cars;
            ViewBag.CarCount = cars.Count();

            return View(brand);
        }

        // GET: /Brand/Create
        public IActionResult Create()
        {
            var brand = new Brand
            {
                Cars = new List<Car>() // Khởi tạo collection rỗng
            };
            return View(brand);
        }


        // POST: /Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên hãng có trùng không
                if (await _brandRepository.ExistsByNameAsync(brand.Name))
                {
                    ModelState.AddModelError("Name", "Tên hãng xe này đã tồn tại.");
                    return View(brand);
                }

                await _brandRepository.AddAsync(brand);
                TempData["SuccessMessage"] = "Thêm hãng xe mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: /Brand/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: /Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra tên hãng có trùng không (không tính chính nó)
                var existingBrand = await _brandRepository.GetByIdAsync(id);
                if (existingBrand.Name != brand.Name && await _brandRepository.ExistsByNameAsync(brand.Name))
                {
                    ModelState.AddModelError("Name", "Tên hãng xe này đã tồn tại.");
                    return View(brand);
                }

                try
                {
                    await _brandRepository.UpdateAsync(brand);
                    TempData["SuccessMessage"] = "Cập nhật hãng xe thành công!";
                }
                catch
                {
                    if (!await _brandRepository.ExistsAsync(id))
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
            return View(brand);
        }

        // GET: /Brand/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            // Kiểm tra xem hãng có xe không
            var carCount = await _brandRepository.GetCarCountAsync(id);
            ViewBag.CarCount = carCount;
            ViewBag.HasCars = carCount > 0;

            return View(brand);
        }

        // POST: /Brand/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            // Kiểm tra xem hãng có xe không, nếu có thì không cho xóa
            var carCount = await _brandRepository.GetCarCountAsync(id);
            if (carCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa hãng xe này vì có xe thuộc hãng đang tồn tại.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _brandRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa hãng xe thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Brand/Cars/5
        public async Task<IActionResult> Cars(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            var cars = await _brandRepository.GetCarsByBrandAsync(id);
            ViewBag.BrandName = brand.Name;
            ViewBag.BrandId = brand.Id;

            return View(cars);
        }

        // GET: /Brand/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var brands = await _brandRepository.SearchAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;

            // Lấy số lượng xe cho mỗi hãng
            var brandList = brands.ToList();
            var brandStats = new Dictionary<int, int>();

            foreach (var brand in brandList)
            {
                brandStats[brand.Id] = await _brandRepository.GetCarCountAsync(brand.Id);
            }

            ViewBag.BrandStats = brandStats;

            return View("Index", brandList);
        }
    }
}
