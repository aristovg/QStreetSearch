using System;
using System.Collections.Generic;
using System.Linq;

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
            var sortedKey = AlphabetSort(key);

            List<DistanceSearchResult<T>> wordsByDistance = new List<DistanceSearchResult<T>>();
            foreach (var knownKey in _wordSet.Keys)
            {
                int distance = Levenshtein.Calculate(sortedKey, knownKey.Value);

                if (distance < distanceLimit)
                {
                    wordsByDistance.AddRange(_wordSet[knownKey].Select(item => new DistanceSearchResult<T>(distance, knownKey.Id, item)));
                }
            }

            return wordsByDistance.OrderBy(x => x.Distance).ToList();
        }

        private static string AlphabetSort(string s) => new string(s.OrderBy(x => x).ToArray());
    }

    internal struct ComparisonKey
    {
        public readonly string Id;
        public readonly string Value;

        public ComparisonKey(string id, string value)
        {
            Id = id;
            Value = value;
        }

        public bool Equals(ComparisonKey other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ComparisonKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }

}
