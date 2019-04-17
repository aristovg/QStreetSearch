using System;
using System.Collections.Generic;
using System.Linq;

namespace AnagramHelper.Parser
{
    public class Street
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

        internal Street(LocalizedOsmStreet street)
        {
            if (!TypesByLanguage.TryGetValue(street.Language, out _knownStreetTypes))
            {
                throw new InvalidOperationException($"Unknown language {street.Language.ToString()}");
            }

            (Name, Type) = TrimStreetType(street.FullName);

            if (!string.IsNullOrWhiteSpace(street.FullOldName))
            {
                (OldName, OldType) = TrimStreetType(street.FullOldName);
            }

            Suburb = street.Suburb;
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