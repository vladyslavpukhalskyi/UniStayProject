namespace Domain.Listings
{
    public class OstrohLandmark
    {
        public OstrohLandmarkId Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        
        private OstrohLandmark(OstrohLandmarkId id, string name, string address, double latitude, double longitude)
        {
            Id = id;
            Name = name;
            Address = address;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static OstrohLandmark Create(OstrohLandmarkId id, string name, string address, double latitude, double longitude)
        {
            return new OstrohLandmark(id, name, address, latitude, longitude);
        }
        
        public double CalculateDistanceTo(double latitude, double longitude)
        {
            const double EarthRadiusKm = 6371.0;

            var lat1Rad = Latitude * Math.PI / 180.0;
            var lat2Rad = latitude * Math.PI / 180.0;
            var deltaLat = (latitude - Latitude) * Math.PI / 180.0;
            var deltaLon = (longitude - Longitude) * Math.PI / 180.0;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }
    }
}
