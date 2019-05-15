namespace QStreetSearch.Contracts
{
    public class GeoNode
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeoNode(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}