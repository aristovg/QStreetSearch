using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QStreetSearch.Contracts;

namespace OsmFilterOutputConverter
{

    [Verb("streets")]
    class StreetOptions : Options
    {

    }

    [Verb("places")]
    class PlacesOptions : Options
    {

    }

    class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input files to be processed.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Destination file.")]
        public string OutputFile { get; set; }
    }

    class Program
    {
        private static ILogger _logger;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddConsole()).BuildServiceProvider();
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("default");

            Parser.Default.ParseArguments<StreetOptions, PlacesOptions>(args)
                .MapResult(
                     async (StreetOptions opts) => await WriteStreets(opts),
                     async (PlacesOptions opts) => await WritePlaces(opts),
                    errs => Task.FromResult(1));

            Console.ReadLine();
        }

        public class OsmWay
        {
            public OsmName Name { get; set; }
            public OsmName Ua { get; set; }
            public OsmName Ru { get; set; }

            public string ParentDistrict { get; set; }

            public List<OsmGeoNode> Nodes { get; set; }
        }

        static async Task WriteStreets(Options options)
        {
            await ProcessFile(options, ParseStreets);
        }

        static async Task WritePlaces(Options options)
        {
            await ProcessFile(options, ParsePlaces);
        }

        static async Task<int> ProcessFile(Options options, Func<XElement, IEnumerable<object>> parser)
        {
            _logger.LogInformation($"Input file {options.InputFile}");

            using (var fs = File.OpenText(options.InputFile))
            {
                var root = await XElement.LoadAsync(fs, LoadOptions.None, CancellationToken.None);

                var distinctGeoObjects = parser(root);

                using (var sw = new StreamWriter(options.OutputFile))
                {
                    var serializer = new JsonSerializer()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    serializer.Serialize(sw, distinctGeoObjects);
                }
            }

            _logger.LogInformation($"Output file {options.OutputFile}. Done");

            return 0;
        }

        private static List<OsmGeoObject> ParseStreets(XElement root)
        {
            var nodes = (from node in root.Descendants("node")
                select new OsmGeoNode()
                {
                    Id = (string) node.Attribute("id"),
                    Latitude = (double) node.Attribute("lat"),
                    Longitude = (double) node.Attribute("lon")
                }).ToDictionary(x => x.Id, x => x);


            var objects = from way in root.Descendants("way")
                let fullName = GetTagValue(way, "name")
                where fullName != null
                let wayNodes = (from nodeRef in way.Elements("nd") select (string) nodeRef.Attribute("ref")).ToList()
                select new OsmWay
                {
                    Name = GetOsmName(way),
                    Nodes = GetGeoNodes(fullName, nodes, wayNodes),
                    Ua = GetOsmName(way, "uk"),
                    Ru = GetOsmName(way, "ru"),
                    ParentDistrict = GetTagValue(way, "addr:suburb")
                };

            var wayGroups = objects.GroupBy(x => new {x.Name.FullName, x.ParentDistrict});

            var distinctGeoObjects = new List<OsmGeoObject>();

            foreach (var wayGroup in wayGroups)
            {
                string SelectMostFrequent(Func<OsmWay, string> selector)
                {
                    return wayGroup.Select(selector)
                        .GroupBy(x => x)
                        .OrderByDescending(x => x.Count())
                        .First()
                        .Key;
                }

                var osmGeoObject = new OsmGeoObject
                {
                    Nodes = wayGroup.SelectMany(x => x.Nodes).ToList(),
                    Ua = new OsmName()
                    {
                        FullOldName = SelectMostFrequent(x => x.Ua.FullOldName),
                        FullName = SelectMostFrequent(x => x.Ua.FullName)
                    },
                    Ru = new OsmName()
                    {
                        FullOldName = SelectMostFrequent(x => x.Ru.FullOldName),
                        FullName = SelectMostFrequent(x => x.Ru.FullName)
                    },
                    ParentDistrict = wayGroup.Key.ParentDistrict
                };

                distinctGeoObjects.Add(osmGeoObject);
            }

            return distinctGeoObjects;
        }

        private static IEnumerable<OsmGeoObject> ParsePlaces(XElement root)
        {
            var nodes = from node in root.Descendants("node")
                select new OsmGeoObject()
                {
                    Ua = GetOsmName(node, "uk"),
                    Ru = GetOsmName(node, "ru"),
                    Nodes = new List<OsmGeoNode>()
                    {
                        new OsmGeoNode()
                        {
                            Latitude = (double) node.Attribute("lat"),
                            Longitude = (double) node.Attribute("lon")
                        }
                    }

                };

            return nodes;
        }

        private static OsmName GetOsmName(XElement element, string language = null)
        {
            return new OsmName
            {
                FullName = GetLocalizedName(element, "name", language),
                FullOldName = GetLocalizedName(element, "old_name", language)
            };
        }

        private static List<OsmGeoNode> GetGeoNodes(string fullName, Dictionary<string, OsmGeoNode> nodes, List<string> nodeIds)
        {
            var list = new List<OsmGeoNode>(nodeIds.Count);

            foreach (var id in nodeIds)
            {
                if (!nodes.TryGetValue(id, out var node))
                {
                    _logger.LogWarning($"Node {id} for object {fullName} was not found in the input file");
                }
                else
                {
                    list.Add(node);
                }
            }

            if (!list.Any())
            {
                _logger.LogError($"All nodes lookup failed for object {fullName}");
            }

            return list;
        }

        private static string GetLocalizedName(XElement element, string tagKey, string language = null)
        {
            if (language == null) return GetTagValue(element, tagKey);

            var localizedTag = $"{tagKey}:{language}";
            var localizedValue = GetTagValue(element, localizedTag);

            return !string.IsNullOrWhiteSpace(localizedValue) ? localizedValue : GetTagValue(element, tagKey);
        }

        private static string GetTagValue(XElement element, string tagKey)
        {
            return (from tag in element.Elements("tag")
                where (string) tag.Attribute("k") == tagKey
                select (string) tag.Attribute("v")).FirstOrDefault();
        }
    }
}
