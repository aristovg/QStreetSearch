namespace QStreetSearch.Search
{
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