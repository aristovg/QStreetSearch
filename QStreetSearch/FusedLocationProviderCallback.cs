using System.Linq;
using Android.Gms.Location;
using Android.Util;

namespace QStreetSearch
{
    public class FusedLocationProviderCallback : LocationCallback
    {
        public readonly MainActivity _activity;

        public FusedLocationProviderCallback(MainActivity activity)
        {
            this._activity = activity;
        }



        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                _activity.UpdateLocation(location);
            }
            else
            {
                _activity.TriggerLocationUnavailable();
            }
        }
    }
}