using System.Collections.Generic;
using Newtonsoft.Json;

namespace steamBackup
{
    public class ConfigFile
    {
        [JsonProperty("Archiver Version")]
        public int ArchiverVersion { get; set; }

        [JsonProperty("ACF IDs")]
        public Dictionary<string, string> AcfIds { get; set; } 
    }
}