using System.Collections.Generic;

namespace QStreetSearch.Search
{
    public class SimpleContainsSearch<T> 
    {
        private readonly Dictionary<ComparisonKey, T> _wordSet = new Dictionary<ComparisonKey, T>();

        public SimpleContainsSearch(IEnumerable<T> data, IEnumerable<ComparisonKeySelector<T>> comparisonSelectors)
        {
            foreach (var item in data)
            {
                foreach (var selector in comparisonSelectors)
                {
                    var key = selector.SelectorFunc(item);

                    if (string.IsNullOrWhiteSpace(key)) continue;

                    var normalizedKey = key.ToLowerInvariant();

                    var comparisonKey = new ComparisonKey(selector.Id, normalizedKey);
                    if (!_wordSet.ContainsKey(comparisonKey))
                    {
                        _wordSet.Add(comparisonKey, item);
                    }
                }
            }
        }

        public List<SearchResult<T>> FindByContainsSequence(string sequence)
        {
            var normalizedSequence = sequence.ToLower();

            List<SearchResult<T>> searchResult = new List<SearchResult<T>>();

            foreach (var knownKey in _wordSet.Keys)
            {
                if (knownKey.Value.Contains(normalizedSequence))
                {
                    searchResult.Add(new SearchResult<T>(knownKey.Id, _wordSet[knownKey]));
                }
            }

            return searchResult;
        }
    }
}