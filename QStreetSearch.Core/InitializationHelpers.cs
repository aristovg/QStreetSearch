using System.Collections.Generic;

namespace QStreetSearch.Search
{
    internal class InitializationHelpers
    {
        public static Dictionary<ComparisonKey, TItem> InitializeWithLowercaseKeys<TItem>(IEnumerable<TItem> items,
            IEnumerable<ComparisonKeySelector<TItem>> comparisonKeySelectors)
        {

            var dictionary = new Dictionary<ComparisonKey, TItem>();
            foreach (var item in items)
            {
                foreach (var comparisonKeySelector in comparisonKeySelectors)
                {
                    var key = comparisonKeySelector.SelectorFunc(item);

                    if (string.IsNullOrEmpty(key)) continue;

                    string normalizedKey = key.ToLower();
                    var comparisonKey = new ComparisonKey(comparisonKeySelector.Id, normalizedKey);
                    if (!dictionary.ContainsKey(comparisonKey))
                    {
                        dictionary.Add(comparisonKey, item);
                    }
                }
            }

            return dictionary;
        }
    }
}