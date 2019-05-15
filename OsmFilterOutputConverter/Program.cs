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

            Parser.Default.ParseArguments<Options>(args).WithParsed(async opts => await ProcessFile(opts));

            Console.ReadLine();
        }

        static async Task ProcessFile(Options options)
        {
            _logger.LogInformation($"Input file {options.InputFile}");

            using (var fs = File.OpenText(options.InputFile))
            {
                var root = await XElement.LoadAsync(fs, LoadOptions.None, CancellationToken.None);

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
                    select new OsmGeoObject
                    {
                        Nodes = GetGeoNodes(fullName, nodes, wayNodes),
                        Ua = GetOsmName(way, "ua"),
                        Ru = GetOsmName(way, "ru"),
                        ParentDistrict = GetTagValue(way, "addr:suburb")
                    };

                using (var sw = new StreamWriter(options.OutputFile))
                {
                    var serializer = new JsonSerializer()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    serializer.Serialize(sw, objects);
                }
            }
        }

        private static OsmName GetOsmName(XElement way, string language)
        {
            return new OsmName
            {
                FullName = GetLocalizedName(way, "name", language),
                FullOldName = GetLocalizedName(way, "old_name", language)
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

        private static string GetLocalizedName(XElement element, string tagKey, string language)
        {
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
