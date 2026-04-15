using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hangman.Models
{
    public class WordBank
    {
        [JsonPropertyName("categories")]
        public List<WordCategory> Categories { get; set; } = new();
    }

    public class WordCategory
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("words")]
        public List<string> Words { get; set; } = new();
    }
}