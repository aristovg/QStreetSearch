using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QStreetSearch.Search
{
    public class PatternSearch<T>
    {
        private readonly Dictionary<ComparisonKey, T> _wordSet;

        public PatternSearch(IEnumerable<T> items, IEnumerable<ComparisonKeySelector<T>> comparisonKeySelectors)
        {
            _wordSet = InitializationHelpers.InitializeWithLowercaseKeys(items, comparisonKeySelectors);
        }

        public PatternSearch(IEnumerable<T> items, ComparisonKeySelector<T> comparisonKeySelectorSelector)
            : this(items, new[] { comparisonKeySelectorSelector })
        {

        }

        public List<SearchResult<T>> FindByPattern(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var regexPattern = key.ToLower().Replace('?', '.').Replace("*", ".*");

            var regex = new Regex($"^{regexPattern}$");

            List<SearchResult<T>> wordsByDistance = new List<SearchResult<T>>();

            foreach (var knownKey in _wordSet.Keys)
            {
                if (regex.IsMatch(knownKey.Value))
                {
                    wordsByDistance.Add(new SearchResult<T>(knownKey.Id, _wordSet[knownKey]));
                }
            }

            return wordsByDistance;
        }
    }
}