using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QStreetSearch.Contracts
{
    public class OsmGeoNode
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lon")]
        public double Longitude { get; set; }
    }

    public class OsmGeoObject
    {
        public OsmName Ua { get; set; }
        public OsmName Ru { get; set; }

        public string ParentDistrict { get; set; }

        public List<OsmGeoNode> Nodes { get; set; }
    }
}
