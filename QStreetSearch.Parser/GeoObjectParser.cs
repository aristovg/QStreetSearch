using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace QStreetSearch.Parser
{
    public class GeoObjectParser
    {
        private static readonly Dictionary<Language, Func<OsmGeoObject, LocalizedOsmGeoObject>> Parsers =
            new Dictionary<Language, Func<OsmGeoObject, LocalizedOsmGeoObject>>()
            {
                [Language.Ru] = str => new LocalizedOsmGeoObject(Language.Ru, str.NameRu, str.OldNameRu, str.Suburb),
                [Language.Ua] = str => new LocalizedOsmGeoObject(Language.Ua, str.NameUa, str.OldNameUa, str.Suburb)
            };

        public static IEnumerable<GeoObject> Parse(Stream stream, IEnumerable<Language> languages)
        {
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = "\t";
                csv.Configuration.BadDataFound = null;
                var geoObjects = csv.GetRecords<OsmGeoObject>().SelectMany(_ => languages, (street, lang) => Parsers[lang](street));

                return geoObjects.Select(x => new GeoObject(x)).ToList();
            }
        }
    }
}
