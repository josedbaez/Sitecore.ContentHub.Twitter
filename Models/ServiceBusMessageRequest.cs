using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sitecore.ContentHub.Twitter.Models
{
    [JsonObject]
    [Serializable]
    public class ServiceBusMessageRequest
    {
        [JsonProperty("saveEntityMessage")]
        public EntityMessage Message { get; set; }
    }

    public class EntityMessage
    {
        [JsonProperty("TargetId")]
        public long TargetId { get; set; }

        [JsonProperty("ChangeSet")]
        public ChangeSet ChangeSet { get; set; }
    }

    public class ChangeSet
    {
        [JsonProperty("Cultures")]
        public IEnumerable<string> Cultures { get; set; }
    }
}
