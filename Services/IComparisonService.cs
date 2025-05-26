namespace GotoCarRental.Services
{
    public interface IComparisonService
    {
        /// <summary>
        /// Lấy danh sách ID xe đang được so sánh
        /// </summary>
        List<int> GetComparisonList();

        /// <summary>
        /// Thêm xe vào danh sách so sánh
        /// </summary>
        bool AddToComparison(int carId);

        /// <summary>
        /// Xóa xe khỏi danh sách so sánh
        /// </summary>
        bool RemoveFromComparison(int carId);

        /// <summary>
        /// Xóa tất cả xe khỏi danh sách so sánh
        /// </summary>
        void ClearComparison();

        /// <summary>
        /// Kiểm tra xe có trong danh sách so sánh hay không
        /// </summary>
        bool IsInComparison(int carId);
    }
}
