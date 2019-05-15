using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using QStreetSearch.Parser;
using QStreetSearch.Search;
using Android.Gms.Location;
using Android.Support.V4.App;
using QStreetSearch.Location;
using Environment = System.Environment;

namespace QStreetSearch
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private class SearchStrategy
        {
            public Func<string, IEnumerable<SearchResult<GeoObject>>> SearchFunc { get; }

            public bool Ordered { get; }

            public SearchStrategy(Func<string, IEnumerable<SearchResult<GeoObject>>> searchFunc, bool ordered = false)
            {
                SearchFunc = searchFunc;
                Ordered = ordered;
            }
        }

        private static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;
        private static readonly int RC_LOCATION_UPDATES_PERMISSION_CHECK = 1100;

        private const string CurrentNameKey = "Current";
        private const string OldNameKey = "Old";

        private const int DefaultItemsCount = 100;

        private Lazy<AnagramDistanceSearch<GeoObject>> _anagramDistanceSearch;
        private Lazy<SimpleContainsSearch<GeoObject>> _containsSearch;
        private Lazy<DistanceSearch<GeoObject>> _distanceSearch;
        private Lazy<PatternSearch<GeoObject>> _patternSearch;

        private Dictionary<string, SearchStrategy> _searchStrategies;
        private Dictionary<string, Func<List<GeoObject>>> _dataSetsFetchers;

        private SearchStrategy _selectedSearchStrategy;
        private List<GeoObject> _workingDataSet;

        private readonly ComparisonKeySelector<GeoObject>[] _comparisonKeys = new[]
        {
            new ComparisonKeySelector<GeoObject>(CurrentNameKey, x => x.Name),
            new ComparisonKeySelector<GeoObject>(OldNameKey, x => x.OldName)
        };

        private FusedLocationProviderClient _fusedLocationProviderClient;
        private TextView _locationTextView;
        private LocationRequest _locationRequest;

        private Android.Locations.Location _currentLocation;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            Window.RequestFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_main);

            if (IsGooglePlayServicesInstalled())
            {
                _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
                _locationTextView = FindViewById<TextView>(Resource.Id.textViewCoordinates);

                _locationRequest = new LocationRequest()
                    .SetPriority(LocationRequest.PriorityHighAccuracy)
                    .SetInterval(5 * 1000)
                    .SetFastestInterval(5 * 1000);

                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, RC_LOCATION_UPDATES_PERMISSION_CHECK);
            }

            InitializeDataSetFetchers();

            InitializeSearchStrategies();

            InitializeSearchAlgorithms();

            SetupDataSetSpinner();
            SetupSearchMethodSpinner();
            SetupSearchEditText();
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == RC_LAST_LOCATION_PERMISSION_CHECK || requestCode == RC_LOCATION_UPDATES_PERMISSION_CHECK)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                        await _fusedLocationProviderClient.RequestLocationUpdatesAsync(_locationRequest, new FusedLocationProviderCallback(this));
                    
                }
                else
                {
                    Toast.MakeText(this, "Error receiving GPS permissions", ToastLength.Long);
                    return;
                }
            }
            else
            {
                Log.Debug("FusedLocationProviderSample", "Don't know how to handle requestCode " + requestCode);
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void InitializeSearchAlgorithms()
        {
            _anagramDistanceSearch = new Lazy<AnagramDistanceSearch<GeoObject>>(() => new AnagramDistanceSearch<GeoObject>(_workingDataSet, _comparisonKeys));
            _containsSearch = new Lazy<SimpleContainsSearch<GeoObject>>(() => new SimpleContainsSearch<GeoObject>(_workingDataSet, _comparisonKeys));
            _distanceSearch = new Lazy<DistanceSearch<GeoObject>>(() => new DistanceSearch<GeoObject>(_workingDataSet, _comparisonKeys));
            _patternSearch = new Lazy<PatternSearch<GeoObject>>(() => new PatternSearch<GeoObject>(_workingDataSet, _comparisonKeys));
        }

        private void InitializeDataSetFetchers()
        {
            _dataSetsFetchers = new Dictionary<string, Func<List<GeoObject>>>()
            {
                [Resources.GetString(Resource.String.dataset_kyiv_region_ru)] = () => DataSetFetcher("kyiv.json", Language.Ru),
                [Resources.GetString(Resource.String.dataset_kyiv_region_ua)] = () => DataSetFetcher("kyiv.json", Language.Ua),
                [Resources.GetString(Resource.String.dataset_kyiv_region_ru_ua)] = () => DataSetFetcher("kyiv.json", Language.Ru, Language.Ua),
                [Resources.GetString(Resource.String.dataset_kyiv_street_ru)] = () => DataSetFetcher("kyiv.json", Language.Ru),
                [Resources.GetString(Resource.String.dataset_kyiv_street_ua)] = () => DataSetFetcher("kyiv.json", Language.Ua),
                [Resources.GetString(Resource.String.dataset_kyiv_street_ru_ua)] = () => DataSetFetcher("kyiv.json", Language.Ru, Language.Ua),
            };

            List<GeoObject> DataSetFetcher(string assetName, params Language[] languages)
            {
                var streetStream = Assets.Open(assetName);
                return GeoObjectParser.Parse(streetStream, languages).ToList();
            }
        }

        private void InitializeSearchStrategies()
        {
            _searchStrategies = new Dictionary<string, SearchStrategy>()
            {
                [Resources.GetString(Resource.String.method_contains)] =
                    new SearchStrategy(s => _containsSearch.Value.FindByContainsSequence(s)),
                [Resources.GetString(Resource.String.method_anagramdistance)] =
                    new SearchStrategy(s => _anagramDistanceSearch.Value.FindByDistance(s), true),
                [Resources.GetString(Resource.String.method_distance)] =
                    new SearchStrategy(s => _distanceSearch.Value.FindByDistance(s)),
                [Resources.GetString(Resource.String.method_pattern)] =
                    new SearchStrategy(s => _patternSearch.Value.FindByPattern(s))
            };
        }

        private void SetupSearchEditText()
        {
            EditText editTextQuery = FindViewById<EditText>(Resource.Id.editTextQuery);
            editTextQuery.EditorAction += (sender, e) =>
            {
                e.Handled = false;

                if (e.ActionId == ImeAction.Done)
                {
                    string text = editTextQuery.Text;

                    string clearedText = new string(text.Where(char.IsLetterOrDigit).ToArray());

                    var results = _selectedSearchStrategy.SearchFunc(clearedText)
                        .Take(DefaultItemsCount)
                        .Select(searchResult => new
                        {
                            SearchResult = searchResult,
                            Distance = _currentLocation?.GetDistanceTo(searchResult.Item.GeoNodes)
                        });
                        
                    if (_currentLocation != null && !_selectedSearchStrategy.Ordered)
                    {
                        results = results.OrderBy(x => x.Distance.Min);
                    }

                    var formattedResults = results.Select(x => FormatListViewItem(x.SearchResult, x.Distance)).ToList();

                    ListView listView = FindViewById<ListView>(Resource.Id.listViewResults);
                    listView.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, formattedResults);

                    e.Handled = true;
                }
            };
        }

        private void SetupSearchMethodSpinner()
        {
            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinnerSearchMethod);
            spinner.ItemSelected += (sender, args) =>
            {
                var item = (string) spinner.GetItemAtPosition(args.Position);
                _selectedSearchStrategy = _searchStrategies[item];
            };
            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.search_methods_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }

        private void SetupDataSetSpinner()
        {
            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinnerDataSet);
            spinner.ItemSelected += (sender, args) =>
            {
                var item = (string)spinner.GetItemAtPosition(args.Position);
                var fetcher = _dataSetsFetchers[item];
                _workingDataSet = fetcher();
                InitializeSearchAlgorithms();
            };

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.dataset_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }

        private static string FormatListViewItem(SearchResult<GeoObject> searchResult, DistanceToObject distanceToObject)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(searchResult.Item.Name);

            sb.Append(string.IsNullOrEmpty(searchResult.Item.Suburb)
                ? $" [{searchResult.Item.Type}]"
                : $" [{searchResult.Item.Type}, {searchResult.Item.Suburb}]");

            var distanceSearch = searchResult as DistanceSearchResult<GeoObject>;

            if (distanceSearch != null && searchResult.KeyId == CurrentNameKey)
            {
                sb.Append($" (word dist: {distanceSearch.Distance})");
            }

            if (!string.IsNullOrEmpty(searchResult.Item.OldName))
            {
                sb.Append(Environment.NewLine);
                sb.Append($"Old: {searchResult.Item.OldName} [{searchResult.Item.OldType}]");

                if (distanceSearch != null && searchResult.KeyId == OldNameKey)
                {
                    sb.Append($" (word dist: {distanceSearch.Distance})");
                }
            }

            if (distanceToObject != null && !distanceToObject.IsEmpty)
            {
                string Format(float f) => (f / 1000).ToString("F1");
                sb.Append(Environment.NewLine);
                sb.Append($"{Format(distanceToObject.Min)} - {Format(distanceToObject.Max)} (Avg: {Format(distanceToObject.Avg)}) (km)");
            }

            return sb.ToString();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Toast.MakeText(this, "Google Play Services is installed on this device.", ToastLength.Long);
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);

                Toast.MakeText(this, "Google Play Services is NOT installed on this device.", ToastLength.Long);

                Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                    queryResult, errorString);
            }

            return false;
        }

        public void TriggerLocationUnavailable()
        {
            Toast.MakeText(this, "Location is unavailable", ToastLength.Long);
        }

        public void UpdateLocation(Android.Locations.Location location)
        {
            _currentLocation = location;
            _locationTextView.SetText($"{location.Latitude} {location.Longitude}", TextView.BufferType.Normal);
        }
    }
}

