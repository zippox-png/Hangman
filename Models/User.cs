using System.Text.Json.Serialization;

namespace Hangman.Models
{
    public class User
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("imagePath")]
        public string ImagePath { get; set; } = string.Empty;
    }
}