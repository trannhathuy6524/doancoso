namespace GotoCarRental.Models
{
    public class CarComparisonViewModel
    {
        public List<Car> Cars { get; set; } = new List<Car>();
        public List<Feature> AllFeatures { get; set; } = new List<Feature>();


        public bool HasFeature(int carId, int featureId)
        {
            var car = Cars.FirstOrDefault(c => c.Id == carId);
            if (car == null || car.CarFeatures == null)
                return false;

            return car.CarFeatures.Any(cf => cf.FeatureId == featureId);
        }
    }
}
