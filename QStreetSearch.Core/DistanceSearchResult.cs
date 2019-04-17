namespace QStreetSearch.Search
{
    public class SearchResult<T>
    {
        public string KeyId { get; }
        public T Item { get; }

        public SearchResult(string keyId, T item)
        {
            Item = item;
            KeyId = keyId;
        }
    }

    public class DistanceSearchResult<T> : SearchResult<T>
    {
        public int Distance { get; }

        public DistanceSearchResult(int distance, string keyId, T item) : base(keyId, item)
        {
            Distance = distance;
        }
    }
}