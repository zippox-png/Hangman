using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class StatisticsService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "statistics.json");

        public List<UserStatistics> LoadAll()
        {
            if (!File.Exists(_filePath)) return new List<UserStatistics>();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<UserStatistics>>(json) ?? new List<UserStatistics>();
        }

        public void SaveAll(List<UserStatistics> stats)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true }));
        }

        public UserStatistics GetOrCreate(string username)
        {
            var all = LoadAll();
            var existing = all.Find(s => s.Username == username);
            if (existing != null) return existing;
            var newStat = new UserStatistics { Username = username };
            all.Add(newStat);
            SaveAll(all);
            return newStat;
        }

        public void RecordGame(string username, string category, bool won)
        {
            var all = LoadAll();
            var stat = all.Find(s => s.Username == username);
            if (stat == null)
            {
                stat = new UserStatistics { Username = username };
                all.Add(stat);
            }
            if (!stat.CategoryStats.ContainsKey(category))
                stat.CategoryStats[category] = new CategoryStat();
            stat.CategoryStats[category].Played++;
            if (won) stat.CategoryStats[category].Won++;
            SaveAll(all);
        }

        public void DeleteUser(string username)
        {
            var all = LoadAll();
            all.RemoveAll(s => s.Username == username);
            SaveAll(all);
        }
    }
}