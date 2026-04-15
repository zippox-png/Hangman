using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hangman.Models
{
    public class UserStatistics
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("categoryStats")]
        public Dictionary<string, CategoryStat> CategoryStats { get; set; } = new();
    }

    public class CategoryStat
    {
        [JsonPropertyName("played")]
        public int Played { get; set; } = 0;

        [JsonPropertyName("won")]
        public int Won { get; set; } = 0;
    }
}