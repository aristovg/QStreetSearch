using System;
using System.Collections.Generic;
using System.Linq;

namespace QStreetSearch.Parser
{
    public class GeoObject
    {
        public string Name { get; }
        public string OldName { get; }
        public string Type { get; }
        public string OldType { get; }
        public string Suburb { get; }

        private static readonly Dictionary<Language, KnownStreetTypes> TypesByLanguage = new Dictionary<Language, KnownStreetTypes>()
        {
            [Language.Ru] = KnownStreetTypes.Ru,
            [Language.Ua] = KnownStreetTypes.Ua
        };

        private readonly KnownStreetTypes _knownStreetTypes;

        internal GeoObject(LocalizedOsmGeoObject geoObject)
        {
            if (!TypesByLanguage.TryGetValue(geoObject.Language, out _knownStreetTypes))
            {
                throw new InvalidOperationException($"Unknown language {geoObject.Language.ToString()}");
            }

            (Name, Type) = TrimStreetType(geoObject.StreetName.FullName);

            if (!string.IsNullOrWhiteSpace(geoObject.StreetName.FullOldName))
            {
                (OldName, OldType) = TrimStreetType(geoObject.StreetName.FullOldName);
            }

            Suburb = geoObject.ParentDistrict;
        }


        private (string, string) TrimStreetType(string fullStreetName)
        {
            var parts = fullStreetName.Split(' ');

            string maybeStreetType = parts[parts.Length - 1];

            if (_knownStreetTypes.Contains(maybeStreetType))
            {
                string streetName = string.Join(" ", parts.Take(parts.Length - 1));
                return (streetName, maybeStreetType);
            }

            return (fullStreetName, null);
        }
    }
}