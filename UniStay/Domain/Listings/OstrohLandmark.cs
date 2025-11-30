namespace Domain.Listings
{
    public class OstrohLandmark
    {
        public string Name { get; }
        public string Address { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        private OstrohLandmark(string name, string address, double latitude, double longitude)
        {
            Name = name;
            Address = address;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static readonly OstrohLandmark NewCampus = new(
            "Новий корпус Острозької академії",
            "вул. Семінарська, 2",
            50.32917,
            26.51278
        );

        public static readonly OstrohLandmark OldCampus = new(
            "Старий корпус Острозької академії",
            "вул. Семінарська, 2",
            50.32833,
            26.51278
        );

        public static readonly OstrohLandmark ATB = new(
            "АТБ (Супермаркет)",
            "вул. Гальшки Острозької, 1в",
            50.32729,
            26.52463
        );

        public static readonly OstrohLandmark Castle = new(
            "Острозький замок",
            "вул. Академічна, 5",
            50.32626,
            26.52212
        );

        public static readonly OstrohLandmark NovaPoshta = new(
            "Нова Пошта (Відділення №1)",
            "вул. Князів Острозьких, 3",
            50.32849,
            26.51955
        );

        public static readonly OstrohLandmark Ukrposhta = new(
            "Укрпошта (Відділення 35800)",
            "просп. Незалежності, 7",
            50.32951,
            26.52054
        );

        public static readonly OstrohLandmark TatarTower = new(
            "Татарська вежа",
            "вул. Татарська",
            50.3306,
            26.5260
        );

        public static readonly OstrohLandmark BusStation = new(
            "Автовокзал \"Острог\"",
            "просп. Незалежності, 166",
            50.33557,
            26.49400
        );

        public static readonly OstrohLandmark EpiphanyCathedral = new(
            "Богоявленський собор",
            "вул. Академічна, 5в",
            50.32661,
            26.52128
        );

        public static readonly OstrohLandmark AcademicDormitory = new(
            "Гуртожиток \"Академічний дім\" (№5)",
            "просп. Незалежності, 5",
            50.32951,
            26.52054
        );

        public static IReadOnlyList<OstrohLandmark> AllLandmarks => new List<OstrohLandmark>
        {
            NewCampus,
            OldCampus,
            ATB,
            Castle,
            NovaPoshta,
            Ukrposhta,
            TatarTower,
            BusStation,
            EpiphanyCathedral,
            AcademicDormitory
        };

        /// <summary>
        /// Calculates distance in kilometers using Haversine formula
        /// </summary>
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
