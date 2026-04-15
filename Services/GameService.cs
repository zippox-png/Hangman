using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class GameService
    {
        private readonly string _wordsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "words.json");
        private readonly string _savedGamesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "saved_games");
        private readonly Random _rng = new();

        public WordBank LoadWords()
        {
            if (!File.Exists(_wordsPath)) return new WordBank();
            var json = File.ReadAllText(_wordsPath);
            return JsonSerializer.Deserialize<WordBank>(json) ?? new WordBank();
        }

        public List<string> GetCategories()
        {
            var bank = LoadWords();
            return bank.Categories.Select(c => c.Name).ToList();
        }

        public string GetRandomWord(string category)
        {
            var bank = LoadWords();
            List<string> words;
            if (category == "All Categories")
                words = bank.Categories.SelectMany(c => c.Words).ToList();
            else
            {
                var cat = bank.Categories.FirstOrDefault(c => c.Name == category);
                words = cat?.Words ?? new List<string>();
            }
            if (words.Count == 0) return "HANGMAN";
            return words[_rng.Next(words.Count)].ToUpper();
        }

        public void SaveGame(GameState state, string saveName)
        {
            Directory.CreateDirectory(_savedGamesDir);
            var path = Path.Combine(_savedGamesDir, $"{state.Username}_{saveName}.json");
            state.SavedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(path, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
        }

        public List<string> GetSavedGames(string username)
        {
            if (!Directory.Exists(_savedGamesDir)) return new List<string>();
            return Directory.GetFiles(_savedGamesDir, $"{username}_*.json")
                .Select(f => Path.GetFileNameWithoutExtension(f).Substring(username.Length + 1))
                .ToList();
        }

        public GameState? LoadGame(string username, string saveName)
        {
            var path = Path.Combine(_savedGamesDir, $"{username}_{saveName}.json");
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameState>(json);
        }
    }
}