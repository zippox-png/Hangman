using System.Collections.ObjectModel;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class StatRow
    {
        public string Username { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public string WinRate => Played > 0 ? $"{Won * 100 / Played}%" : "N/A";
    }

    public class StatisticsViewModel : BaseViewModel
    {
        public ObservableCollection<StatRow> Rows { get; } = new();

        public StatisticsViewModel()
        {
            var service = new StatisticsService();
            var all = service.LoadAll();
            foreach (var userStat in all)
                foreach (var kv in userStat.CategoryStats)
                    Rows.Add(new StatRow
                    {
                        Username = userStat.Username,
                        Category = kv.Key,
                        Played = kv.Value.Played,
                        Won = kv.Value.Won
                    });
        }
    }
}