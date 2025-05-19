using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category> AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Category>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Categories
                .Where(c => c.Name.Contains(searchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.CarImages)
                .Where(c => c.CategoryId == categoryId && !c.IsDeleted) // Chỉ lọc theo xe chưa xóa
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }


        public async Task<int> GetCarCountAsync(int categoryId)
        {
            return await _context.Cars
                .Where(c => c.CategoryId == categoryId && !c.IsDeleted) // Chỉ đếm xe chưa xóa
                .CountAsync();
        }

    }
}
