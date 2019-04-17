using System.Collections.Generic;
using System.Linq;

namespace QStreetSearch.Search
{
    public class DistanceSearch<T>
    {
        private readonly Dictionary<ComparisonKey, T> _wordSet = new Dictionary<ComparisonKey, T>();

        public DistanceSearch(IEnumerable<T> items, IEnumerable<ComparisonKeySelector<T>> comparisonKeySelectors)
        {
            foreach (var item in items)
            {
                foreach (var comparisonKeySelector in comparisonKeySelectors)
                {
                    var key = comparisonKeySelector.SelectorFunc(item);

                    if (string.IsNullOrEmpty(key)) continue;

                    string normalizedKey = key.ToLower();
                    var comparisonKey = new ComparisonKey(comparisonKeySelector.Id, normalizedKey);
                    if (!_wordSet.ContainsKey(comparisonKey))
                    {
                        _wordSet.Add(comparisonKey, item);
                    }
                }
            }
        }

        public DistanceSearch(IEnumerable<T> items, ComparisonKeySelector<T> comparisonKeySelectorSelector)
            : this(items, new[] { comparisonKeySelectorSelector })
        {

        }

        public List<DistanceSearchResult<T>> FindByDistance(string key, int distanceLimit = int.MaxValue)
        {
            var normalizedKey = key.ToLower();

            List<DistanceSearchResult<T>> wordsByDistance = new List<DistanceSearchResult<T>>();

            foreach (var knownKey in _wordSet.Keys)
            {
                int distance = Levenshtein.Calculate(normalizedKey, knownKey.Value);

                if (distance < distanceLimit)
                {
                    wordsByDistance.Add(new DistanceSearchResult<T>(distance, knownKey.Id, _wordSet[knownKey]);
                }
            }

            return wordsByDistance.OrderBy(x => x.Distance).ToList();
        }
    }
}