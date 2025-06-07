using System.Text.Json;

namespace GotoCarRental.Services
{
    public class ComparisonService : IComparisonService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _sessionKey = "CarComparison";
        private readonly int _maxCars = 4;

        public ComparisonService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public List<int> GetComparisonList()
        {
            var session = HttpContext?.Session;
            if (session == null)
                return new List<int>();

            string jsonList = session.GetString(_sessionKey);
            if (string.IsNullOrEmpty(jsonList))
                return new List<int>();

            return JsonSerializer.Deserialize<List<int>>(jsonList) ?? new List<int>();
        }

        public bool AddToComparison(int carId)
        {
            var session = HttpContext?.Session;
            if (session == null)
                return false;

            var comparisonList = GetComparisonList();

            // Check if car is already in the list
            if (comparisonList.Contains(carId))
                return false;

            // Check if we reached the limit
            if (comparisonList.Count >= _maxCars)
                return false;

            // Add car to the list
            comparisonList.Add(carId);

            // Save to session
            session.SetString(_sessionKey, JsonSerializer.Serialize(comparisonList));
            return true;
        }

        public bool RemoveFromComparison(int carId)
        {
            var session = HttpContext?.Session;
            if (session == null)
                return false;

            var comparisonList = GetComparisonList();

            // Check if car is in the list
            if (!comparisonList.Contains(carId))
                return false;

            // Remove car from the list
            comparisonList.Remove(carId);

            // Save to session
            session.SetString(_sessionKey, JsonSerializer.Serialize(comparisonList));
            return true;
        }

        public void ClearComparison()
        {
            var session = HttpContext?.Session;
            if (session != null)
            {
                session.Remove(_sessionKey);
            }
        }

        public bool IsInComparison(int carId)
        {
            return GetComparisonList().Contains(carId);
        }
    }
}
