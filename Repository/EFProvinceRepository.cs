using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFProvinceRepository : IProvinceRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProvinceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Province>> GetAllAsync()
        {
            return await _context.Provinces.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Province> GetByIdAsync(int id)
        {
            return await _context.Provinces.FindAsync(id);
        }
    }
}