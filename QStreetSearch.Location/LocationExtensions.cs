using System.Collections.Generic;
using QStreetSearch.Contracts;

namespace QStreetSearch.Location
{
    public static class LocationExtensions
    {

        public static DistanceToObject GetDistanceTo(this Android.Locations.Location source, IEnumerable<GeoNode> targets)
        {
            var distances = new List<float>();

            foreach (var geoNode in targets)
            {
                var androidLocation = new Android.Locations.Location(nameof(LocationExtensions))
                {
                    Latitude = geoNode.Latitude,
                    Longitude = geoNode.Longitude
                };

                var distance = source.DistanceTo(androidLocation);
                distances.Add(distance);
            }

            return new DistanceToObject(distances);
        }
    }
}
