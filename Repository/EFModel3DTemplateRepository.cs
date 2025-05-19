using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFModel3DTemplateRepository : IModel3DTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public EFModel3DTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Model3DTemplate> GetByIdAsync(int id)
        {
            return await _context.Model3DTemplates
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Model3DTemplate>> GetAllAsync()
        {
            return await _context.Model3DTemplates
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Model3DTemplate>> GetByBrandAndCategoryAsync(int? brandId, int? categoryId)
        {
            var query = _context.Model3DTemplates
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .AsQueryable();

            if (brandId.HasValue)
            {
                query = query.Where(m => m.BrandId == brandId);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId);
            }

            return await query.OrderBy(m => m.Name).ToListAsync();
        }

        public async Task<Model3DTemplate> AddAsync(Model3DTemplate model3DTemplate)
        {
            model3DTemplate.CreatedAt = DateTime.UtcNow;
            model3DTemplate.UpdatedAt = DateTime.UtcNow;

            await _context.Model3DTemplates.AddAsync(model3DTemplate);
            await _context.SaveChangesAsync();
            return model3DTemplate;
        }

        public async Task UpdateAsync(Model3DTemplate model3DTemplate)
        {
            model3DTemplate.UpdatedAt = DateTime.UtcNow;

            _context.Entry(model3DTemplate).State = EntityState.Modified;
            // Không chỉnh sửa CreatedAt
            _context.Entry(model3DTemplate).Property(x => x.CreatedAt).IsModified = false;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var model3DTemplate = await _context.Model3DTemplates.FindAsync(id);
            if (model3DTemplate != null)
            {
                _context.Model3DTemplates.Remove(model3DTemplate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Model3DTemplates.AnyAsync(m => m.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Model3DTemplates.CountAsync();
        }

        public async Task<int> CountByBrandAsync(int brandId)
        {
            return await _context.Model3DTemplates.CountAsync(m => m.BrandId == brandId);
        }

        public async Task<int> CountByCategoryAsync(int categoryId)
        {
            return await _context.Model3DTemplates.CountAsync(m => m.CategoryId == categoryId);
        }
    }
}
