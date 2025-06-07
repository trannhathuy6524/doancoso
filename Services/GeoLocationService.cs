namespace GotoCarRental.Services
{
    public class GeoLocationService : IGeoLocationService
    {
        // Tính khoảng cách giữa hai tọa độ sử dụng công thức Haversine
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0; // Bán kính Trái Đất (km)

            // Chuyển đổi độ sang radian
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            // Công thức Haversine
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusKm * c;

            return Math.Round(distance, 2); // Làm tròn đến 2 chữ số thập phân
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
