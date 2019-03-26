using System.Collections.Generic;

namespace TechSummit.Demo
{
    public class CosmosDbConfig
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public string DbName { get; set; }
        public string CollectionName { get; set; }
        public bool IsPartitioningEnabled { get; set; } = true;
        public HashSet<string> PreferredLocations { get; set; } = new HashSet<string> { "West Europe" };
    }
}
