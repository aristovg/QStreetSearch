using System;
using System.Collections.Generic;

namespace QStreetSearch.Search
{
    public class SimpleContainsSearch<T> 
    {
        private readonly Dictionary<ComparisonKey, T> _wordSet;

        public SimpleContainsSearch(IEnumerable<T> items, IEnumerable<ComparisonKeySelector<T>> comparisonKeySelectors)
        {
            _wordSet = InitializationHelpers.InitializeWithLowercaseKeys(items, comparisonKeySelectors);

        }

        public List<SearchResult<T>> FindByContainsSequence(string sequence)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));

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