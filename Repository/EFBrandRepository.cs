using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFBrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public EFBrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Brand> GetByIdAsync(int id)
        {
            return await _context.Brands.FindAsync(id);
        }

        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            return await _context.Brands.OrderBy(b => b.Name).ToListAsync();
        }

        public async Task<Brand> AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task UpdateAsync(Brand brand)
        {
            _context.Entry(brand).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Brand>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Brands
                .Where(b => b.Name.Contains(searchTerm))
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Brands.AnyAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name == name);
        }

        public async Task<IEnumerable<Car>> GetCarsByBrandAsync(int brandId)
        {
            return await _context.Cars
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.BrandId == brandId && !c.IsDeleted) // Chỉ lấy những xe chưa bị xóa
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }


        public async Task<int> GetCarCountAsync(int brandId)
        {
            // Chỉ đếm những xe chưa bị xóa (IsDeleted = false)
            return await _context.Cars
                .Where(c => c.BrandId == brandId && !c.IsDeleted)
                .CountAsync();
        }

    }
}
