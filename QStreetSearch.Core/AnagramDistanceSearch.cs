using System;
using System.Collections.Generic;
using System.Linq;

namespace QStreetSearch.Search
{
    public class AnagramDistanceSearch<T>
    {
        private readonly Dictionary<ComparisonKey, List<T>> _wordSet = new Dictionary<ComparisonKey, List<T>>();

        public AnagramDistanceSearch(IEnumerable<T> data, IEnumerable<ComparisonKeySelector<T>> comparisonKeySelectors)
        {
            foreach (var elem in data)
            {
                foreach (var comparisonKeySelector in comparisonKeySelectors)
                {
                    var key = comparisonKeySelector.SelectorFunc(elem);

                    if (string.IsNullOrEmpty(key)) continue;

                    var sortedKey = AlphabetSort(key);

                    if (_wordSet.TryGetValue(new ComparisonKey(comparisonKeySelector.Id, sortedKey), out var list))
                    {
                        list.Add(elem);
                    }
                    else
                    {
                        _wordSet.Add(new ComparisonKey(comparisonKeySelector.Id, sortedKey), new List<T>() {elem});
                    }
                }
            }
        }

        public AnagramDistanceSearch(IEnumerable<T> data, ComparisonKeySelector<T> comparisonKeySelectorSelector) 
            : this(data, new[] {comparisonKeySelectorSelector})
        {

        }


        public List<DistanceSearchResult<T>> FindByDistance(string key, int distanceLimit = int.MaxValue)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var sortedKey = AlphabetSort(key);

            List<DistanceSearchResult<T>> wordsByDistance = new List<DistanceSearchResult<T>>();
            foreach (var comparisonKey in _wordSet.Keys)
            {
                int distance = Levenshtein.Calculate(sortedKey, comparisonKey.Value);

                if (distance < distanceLimit)
                {
                    var results = _wordSet[comparisonKey].Select(item => new DistanceSearchResult<T>(distance, comparisonKey.Id, item));

                    wordsByDistance.AddRange(results);
                }
            }

            return wordsByDistance.OrderBy(x => x.Distance).ToList();
        }

        private static string AlphabetSort(string s) => new string(s.OrderBy(x => x).ToArray());
    }
}
