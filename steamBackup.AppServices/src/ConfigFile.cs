namespace steamBackup.AppServices
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ConfigFile
    {
        [JsonProperty("Archiver Version")]
        public int ArchiverVersion { get; set; }

        [JsonProperty("ACF IDs")]
        public Dictionary<string, string> AcfIds { get; set; } 
    }
}