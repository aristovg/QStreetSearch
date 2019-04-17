using System;

namespace QStreetSearch.Search
{
    public class ComparisonKeySelector<T>
    {
        public string Id { get; }
        public Func<T, string> SelectorFunc { get; }

        public ComparisonKeySelector(string id, Func<T, string> selectorFunc)
        {
            Id = id;
            SelectorFunc = selectorFunc;
        }
    }
}