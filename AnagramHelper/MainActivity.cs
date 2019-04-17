using System.Linq;
using AnagramHelper.Parser;
using AnagramHelper.Search;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace AnagramHelper
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private WordSet<Street> _wordSet;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.RequestFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_main);

            var streetStream = Assets.Open("kiev-streets.csv");
            var parsedStreets = StreetParser.Parse(streetStream, Language.Ru).ToList();

            _wordSet = new WordSet<Street>(parsedStreets, x => x.Name);


            EditText edittext = FindViewById<EditText>(Resource.Id.edittext);
            edittext.EditorAction += (sender, e) => {
                e.Handled = false;

                if (e.ActionId == ImeAction.Done)
                {
                    
                    {
                        Toast.MakeText(this, edittext.Text, ToastLength.Short).Show();
                        e.Handled = true;
                    }

                    string text = edittext.Text;

                    string cleared = new string(text.Where(char.IsLetterOrDigit).ToArray());

                    var streetsByWordDistance = _wordSet.FindByDistance(cleared).Take(20).Select(x => $"{x.Item.Name} [{x.Item.Type}]").ToList();

                    ListView lw = FindViewById<ListView>(Resource.Id.listviewf);
                    lw.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, streetsByWordDistance);
                    edittext.ClearFocus();
                }
            };


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

