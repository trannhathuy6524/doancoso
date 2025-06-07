using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IReviewRepository
    {
        Task<Review> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetAllAsync();
        Task<IEnumerable<Review>> GetByCarIdAsync(int carId);
        Task<IEnumerable<Review>> GetByUserIdAsync(string userId);
        Task<Review> AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasUserReviewedCarAsync(string userId, int carId);
        Task<double> GetAverageRatingForCarAsync(int carId);
        Task<bool> HasUserReviewedRentalAsync(int rentalId, string userId);

    }
}
