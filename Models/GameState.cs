using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hangman.Models
{
    public class GameState
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("guessedLetters")]
        public List<char> GuessedLetters { get; set; } = new();

        [JsonPropertyName("wrongGuesses")]
        public int WrongGuesses { get; set; } = 0;

        [JsonPropertyName("timeRemaining")]
        public double TimeRemaining { get; set; } = 30;

        [JsonPropertyName("currentLevel")]
        public int CurrentLevel { get; set; } = 0;

        [JsonPropertyName("consecutiveWins")]
        public int ConsecutiveWins { get; set; } = 0;

        [JsonPropertyName("savedAt")]
        public string SavedAt { get; set; } = string.Empty;
    }
}