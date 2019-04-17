using System;
using System.Collections.Generic;
using System.Linq;

namespace AnagramHelper.Search
{
    public class WordSet<T>
    {
        private readonly Dictionary<string, List<T>> _wordSet = new Dictionary<string, List<T>>();

        public WordSet(IEnumerable<T> data, Func<T, string> comparisonKeySelector)
        {
            foreach (var elem in data)
            {
                var key = comparisonKeySelector(elem);

                var sortedKey = AlphabetSort(key);

                if (_wordSet.TryGetValue(sortedKey, out var list))
                {
                    list.Add(elem);
                }
                else
                {
                    _wordSet.Add(sortedKey, new List<T>() {elem});
                }
            }
        }

        public bool TryFindExact(string key, out List<T> result)
        {
            var sortedKey = AlphabetSort(key);

            if (_wordSet.TryGetValue(sortedKey, out var values))
            {
                result = values.ToList();
                return true;
            }

            result = new List<T>();
            return false;
        }

        public List<WordDistance<T>> FindByDistance(string key, int distanceLimit = int.MaxValue)
        {
            var sortedKey = AlphabetSort(key);

            List<WordDistance<T>> wordsByDistance = new List<WordDistance<T>>();
            foreach (var knownKey in _wordSet.Keys)
            {
                int distance = Levenshtein.Calculate(sortedKey, knownKey);

                if (distance < distanceLimit)
                {
                    wordsByDistance.AddRange(_wordSet[knownKey].Select(item => new WordDistance<T>(distance, item)));
                }
            }

            return wordsByDistance.OrderBy(x => x.Distance).ToList();
        }

        private static string AlphabetSort(string s) => new string(s.OrderBy(x => x).ToArray());
    }

    public class WordDistance<T>
    {
        public int Distance { get; }
        public T Item { get; }

        public WordDistance(int distance, T item)
        {
            Distance = distance;
            Item = item;
        }
    }
}
