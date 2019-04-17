using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using QStreetSearch.Parser;
using QStreetSearch.Search;

namespace QStreetSearch
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const string CurrentNameKey = "Current";
        private const string OldNameKey = "Old";

        private const int DefaultItemsCount = 100;

        private Lazy<AnagramDistanceSearch<GeoObject>> _anagramDistanceSearch;
        private Lazy<SimpleContainsSearch<GeoObject>> _containsSearch;
        private Lazy<DistanceSearch<GeoObject>> _distanceSearch;
        private Lazy<PatternSearch<GeoObject>> _patternSearch;

        private Dictionary<string, Func<string, IEnumerable<SearchResult<GeoObject>>>> _searchStrategies;
        private Dictionary<string, Func<List<GeoObject>>> _dataSetsFetchers;

        private Func<string, IEnumerable<SearchResult<GeoObject>>> _selectedSearchStrategy;
        private List<GeoObject> _workingDataSet;

        private readonly ComparisonKeySelector<GeoObject>[] _comparisonKeys = new[]
        {
            new ComparisonKeySelector<GeoObject>(CurrentNameKey, x => x.Name),
            new ComparisonKeySelector<GeoObject>(OldNameKey, x => x.OldName)
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.RequestFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_main);

            InitializeDataSetFetchers();

            InitializeSearchStrategies();

            InitializeSearchAlgorithms();

            SetupDataSetSpinner();
            SetupSearchMethodSpinner();
            SetupSearchEditText();
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
                [Resources.GetString(Resource.String.dataset_kyiv_region_ru)] = () => DataSetFetcher("kyivregion.csv", Language.Ru),
                [Resources.GetString(Resource.String.dataset_kyiv_region_ua)] = () => DataSetFetcher("kyivregion.csv", Language.Ua),
                [Resources.GetString(Resource.String.dataset_kyiv_region_ru_ua)] = () => DataSetFetcher("kyivregion.csv", Language.Ru, Language.Ua),
                [Resources.GetString(Resource.String.dataset_kyiv_street_ru)] = () => DataSetFetcher("kiev-streets.csv", Language.Ru),
                [Resources.GetString(Resource.String.dataset_kyiv_street_ua)] = () => DataSetFetcher("kiev-streets.csv", Language.Ua),
                [Resources.GetString(Resource.String.dataset_kyiv_street_ru_ua)] = () => DataSetFetcher("kiev-streets.csv", Language.Ru, Language.Ua),
            };

            List<GeoObject> DataSetFetcher(string assetName, params Language[] languages)
            {
                var streetStream = Assets.Open(assetName);
                return GeoObjectParser.Parse(streetStream, languages).ToList();
            }
        }

        private void InitializeSearchStrategies()
        {
            _searchStrategies = new Dictionary<string, Func<string, IEnumerable<SearchResult<GeoObject>>>>()
            {
                [Resources.GetString(Resource.String.method_contains)] = s => _containsSearch.Value.FindByContainsSequence(s),
                [Resources.GetString(Resource.String.method_anagramdistance)] = s => _anagramDistanceSearch.Value.FindByDistance(s),
                [Resources.GetString(Resource.String.method_distance)] = s => _distanceSearch.Value.FindByDistance(s),
                [Resources.GetString(Resource.String.method_pattern)] = s => _patternSearch.Value.FindByPattern(s)
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

                    string cleared = new string(text.Where(char.IsLetterOrDigit).ToArray());

                    var results = _selectedSearchStrategy(cleared).Take(DefaultItemsCount).Select(FormatListViewItem).ToList();

                    ListView listView = FindViewById<ListView>(Resource.Id.listViewResults);
                    listView.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, results);

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

        private static string FormatListViewItem(SearchResult<GeoObject> searchResult)
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
                sb.Append(System.Environment.NewLine);
                sb.Append($"Old: {searchResult.Item.OldName} [{searchResult.Item.OldType}]");

                if (distanceSearch != null && searchResult.KeyId == OldNameKey)
                {
                    sb.Append($" (word dist: {distanceSearch.Distance})");
                }
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
    }
}

