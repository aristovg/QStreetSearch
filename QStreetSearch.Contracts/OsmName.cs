using Newtonsoft.Json;

namespace QStreetSearch.Contracts
{
    public class OsmName
    {
        [JsonProperty("name")]
        public string FullName { get; set; }

        [JsonProperty("old_name")]
        public string FullOldName { get; set; }
    }
}