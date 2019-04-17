using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace QStreetSearch.Parser
{
    public class StreetParser
    {
        private static Dictionary<Language, Func<OsmStreet, LocalizedOsmStreet>> _parsers = new Dictionary<Language, Func<OsmStreet, LocalizedOsmStreet>>()
        {
            [Language.Ru] = str => new LocalizedOsmStreet(Language.Ru, str.NameRu, str.OldNameRu, str.Suburb),
            [Language.Ua] = str => new LocalizedOsmStreet(Language.Ua, str.NameUa, str.OldNameUa, str.Suburb)
        };

        public static IEnumerable<Street> Parse(Stream stream, Language language)
        {
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = "\t";
                csv.Configuration.BadDataFound = null;
                var streets = csv.GetRecords<OsmStreet>().Select(_parsers[language]).Distinct(LocalizedOsmStreet.FullNameFullOldNameSuburbComparer).ToList();

                return streets.Select(x => new Street(x)).ToList();
            }
        }
    }
}
