namespace TMSWeb.Models
{
    public class GpsMapViewModel
    {
        public string VehicleCode { get; set; } = "";
        public string LicensePlate { get; set; } = "";

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public decimal? Speed { get; set; }
        public DateTime GpsTime { get; set; }
    }
}
