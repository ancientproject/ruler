namespace ruler.Features
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ListOfPackages
    {
        public class RunePackageLite
        {
            [JsonProperty("id")]
            public string ID { get; set; }
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty("status")]
            public MetadataStatusType Status { get; set; }
        }
        [JsonProperty("packages")]
        public List<RunePackageLite> Packages { get; set; } = new List<RunePackageLite>();
    }
}