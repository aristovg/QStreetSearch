using System.Collections.Generic;

namespace QStreetSearch.Parser
{
    internal class LocalizedOsmGeoObject
    {
        public string FullName { get; }
        public string FullOldName { get; }
        public string Suburb { get; }
        public Language Language { get; }

        public LocalizedOsmGeoObject(Language language, string fullName, string fullOldName = null, string suburb = null)
        {
            FullName = fullName.ToLower();
            FullOldName = fullOldName?.ToLower();
            Suburb = suburb?.ToLower();
            Language = language;
        }

        private sealed class FullNameFullOldNameSuburbEqualityComparer : IEqualityComparer<LocalizedOsmGeoObject>
        {
            public bool Equals(LocalizedOsmGeoObject x, LocalizedOsmGeoObject y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.FullName, y.FullName) && string.Equals(x.FullOldName, y.FullOldName) && string.Equals(x.Suburb, y.Suburb);
            }

            public int GetHashCode(LocalizedOsmGeoObject obj)
            {
                unchecked
                {
                    var hashCode = (obj.FullName != null ? obj.FullName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.FullOldName != null ? obj.FullOldName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Suburb != null ? obj.Suburb.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<LocalizedOsmGeoObject> FullNameFullOldNameSuburbComparer { get; } = new FullNameFullOldNameSuburbEqualityComparer();
    }
}