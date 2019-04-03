using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TechSummit.Demo
{
    public class Book
    {
        [Required]
        [JsonProperty("lang")]
        public string Lang { get; set; }

        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }

        [Required]
        [JsonProperty("author")]
        public string Author { get; set; }
    }
}
