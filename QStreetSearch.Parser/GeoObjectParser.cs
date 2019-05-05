using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using QStreetSearch.Contracts;

namespace QStreetSearch.Parser
{
    public class GeoObjectParser
    {
        private static readonly Dictionary<Language, Func<OsmGeoObject, LocalizedOsmGeoObject>> Parsers =
            new Dictionary<Language, Func<OsmGeoObject, LocalizedOsmGeoObject>>()
            {
                [Language.Ru] = obj =>
                    new LocalizedOsmGeoObject(Language.Ru, new StreetName(obj.Ru.FullName, obj.Ru.FullOldName), ConvertGeoNodes(obj), obj.ParentDistrict),
                [Language.Ua] = obj =>
                    new LocalizedOsmGeoObject(Language.Ua, new StreetName(obj.Ua.FullName, obj.Ua.FullOldName), ConvertGeoNodes(obj), obj.ParentDistrict)
            };
    
        private static IEnumerable<GeoNode> ConvertGeoNodes(OsmGeoObject osmGeoObject)
        {
            return osmGeoObject.Nodes.Select(x => new GeoNode(x.Latitude, x.Longitude));
        }

        public static IEnumerable<GeoObject> Parse(Stream stream, IEnumerable<Language> languages)
        {
            using (var reader = new StreamReader(stream))
            {
                var serializer = new JsonSerializer();
                var osmGeoObjects = (List<OsmGeoObject>) serializer.Deserialize(reader, typeof(List<OsmGeoObject>));


                var geoObjects = osmGeoObjects.SelectMany(_ => languages, (street, lang) => Parsers[lang](street));

                return geoObjects.Select(x => new GeoObject(x)).ToList();
            }
        }
    }
}
