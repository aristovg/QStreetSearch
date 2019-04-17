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

        private Lazy<AnagramDistanceSearch<Street>> _anagramDistanceSearch;
        private Lazy<SimpleContainsSearch<Street>> _containsSearch;
        private Lazy<DistanceSearch<Street>> _distanceSearch;

        private Dictionary<string, Func<string, IEnumerable<SearchResult<Street>>>> _searchStrategies;

        private Func<string, IEnumerable<SearchResult<Street>>> _selectedSearchStrategy;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.RequestFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_main);

            InitializeSearchStrategies();

            var streetStream = Assets.Open("kiev-streets.csv");
            var parsedStreets = StreetParser.Parse(streetStream, Language.Ru).ToList();

            var comparisonKeys = new[]
            {
                new ComparisonKeySelector<Street>(CurrentNameKey, x => x.Name),
                new ComparisonKeySelector<Street>(OldNameKey, x => x.OldName)
            };

            _anagramDistanceSearch = new Lazy<AnagramDistanceSearch<Street>>(() => new AnagramDistanceSearch<Street>(parsedStreets, comparisonKeys));
            _containsSearch = new Lazy<SimpleContainsSearch<Street>>(() => new SimpleContainsSearch<Street>(parsedStreets, comparisonKeys));
            _distanceSearch = new Lazy<DistanceSearch<Street>>(() => new DistanceSearch<Street>(parsedStreets, comparisonKeys));

            SetupSearchMethodSpinner();
            SetupSearchEditText();
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

        private void InitializeSearchStrategies()
        {
            _searchStrategies = new Dictionary<string, Func<string, IEnumerable<SearchResult<Street>>>>()
            {
                [Resources.GetString(Resource.String.method_contains)] = s => _containsSearch.Value.FindByContainsSequence(s),
                [Resources.GetString(Resource.String.method_anagramdistance)] = s => _anagramDistanceSearch.Value.FindByDistance(s),
                [Resources.GetString(Resource.String.method_distance)] = s => _distanceSearch.Value.FindByDistance(s),
                [Resources.GetString(Resource.String.method_pattern)] = s => _containsSearch.Value.FindByContainsSequence(s),
            };
        }

        private static string FormatListViewItem(SearchResult<Street> searchResult)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(searchResult.Item.Name);

            sb.Append(string.IsNullOrEmpty(searchResult.Item.Suburb)
                ? $" [{searchResult.Item.Type}]"
                : $" [{searchResult.Item.Type}, {searchResult.Item.Suburb}]");

            var distanceSearch = searchResult as DistanceSearchResult<Street>;

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

