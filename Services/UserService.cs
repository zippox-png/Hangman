using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Hangman.Models;

namespace Hangman.Services
{
    public class UserService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "users.json");

        public List<User> LoadUsers()
        {
            if (!File.Exists(_filePath)) return new List<User>();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void SaveUsers(List<User> users)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void AddUser(User user)
        {
            var users = LoadUsers();
            users.Add(user);
            SaveUsers(users);
        }

        public void DeleteUser(string username)
        {
            var users = LoadUsers();
            users.RemoveAll(u => u.Username == username);
            SaveUsers(users);

            var savedGamesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "saved_games");
            var pattern = $"{username}_*.json";
            if (Directory.Exists(savedGamesDir))
                foreach (var f in Directory.GetFiles(savedGamesDir, pattern))
                    File.Delete(f);
        }

        public bool UserExists(string username)
            => LoadUsers().Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}