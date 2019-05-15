using System.Collections.Generic;
using QStreetSearch.Contracts;

namespace QStreetSearch.Parser
{
    internal class StreetName
    {
        public string FullName { get; }
        public string FullOldName { get; }

        public StreetName(string fullName, string fullOldName = null)
        {
            FullName = fullName.ToLower();
            FullOldName = fullOldName?.ToLower();
        }

        protected bool Equals(StreetName other)
        {
            return string.Equals(FullName, other.FullName) && string.Equals(FullOldName, other.FullOldName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StreetName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FullName != null ? FullName.GetHashCode() : 0) * 397) ^ (FullOldName != null ? FullOldName.GetHashCode() : 0);
            }
        }
    }

    internal class LocalizedOsmGeoObject
    {
        public StreetName StreetName { get; }
        public string ParentDistrict { get; }
        public IEnumerable<GeoNode> GeoNodes { get; }
        public Language Language { get; }

        public LocalizedOsmGeoObject(Language language, StreetName streetName, IEnumerable<GeoNode> geoNodes = null, string suburb = null)
        {
            StreetName = streetName;
            ParentDistrict = suburb?.ToLower();
            GeoNodes = geoNodes;
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
                return string.Equals(x.StreetName, y.StreetName) && string.Equals(x.ParentDistrict, y.ParentDistrict);
            }

            public int GetHashCode(LocalizedOsmGeoObject obj)
            {
                unchecked
                {
                    var hashCode = (obj.StreetName != null ? obj.StreetName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.ParentDistrict != null ? obj.ParentDistrict.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<LocalizedOsmGeoObject> FullNameFullOldNameSuburbComparer { get; } = new LocalizedOsmGeoObject.FullNameFullOldNameSuburbEqualityComparer();
    }
}