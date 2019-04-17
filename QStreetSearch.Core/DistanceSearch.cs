using System;
using System.Collections.Generic;
using System.Linq;

namespace QStreetSearch.Search
{
    public class DistanceSearch<T>
    {
        private readonly Dictionary<ComparisonKey, T> _wordSet;

        public DistanceSearch(IEnumerable<T> items, IEnumerable<ComparisonKeySelector<T>> comparisonKeySelectors)
        {
            _wordSet = InitializationHelpers.InitializeWithLowercaseKeys(items, comparisonKeySelectors);
        }

        public DistanceSearch(IEnumerable<T> items, ComparisonKeySelector<T> comparisonKeySelectorSelector)
            : this(items, new[] { comparisonKeySelectorSelector })
        {

        }

        public List<DistanceSearchResult<T>> FindByDistance(string key, int distanceLimit = int.MaxValue)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var normalizedKey = key.ToLower();

            List<DistanceSearchResult<T>> wordsByDistance = new List<DistanceSearchResult<T>>();

            foreach (var knownKey in _wordSet.Keys)
            {
                int distance = Levenshtein.Calculate(normalizedKey, knownKey.Value);

                if (distance < distanceLimit)
                {
                    wordsByDistance.Add(new DistanceSearchResult<T>(distance, knownKey.Id, _wordSet[knownKey]));
                }
            }

            return wordsByDistance.OrderBy(x => x.Distance).ToList();
        }
    }
}