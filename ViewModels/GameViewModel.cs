using Hangman.Commands;
using Hangman.Models;
using Hangman.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Hangman.ViewModels
{
    public class LetterButton : BaseViewModel
    {
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        public char Letter { get; set; }
    }

    public class GameViewModel : BaseViewModel
    {
        private readonly GameService _gameService = new();
        private readonly StatisticsService _statsService = new();
        private readonly DispatcherTimer _timer = new();

        public User CurrentUser { get; }

        private string _selectedCategory = "All Categories";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                ConsecutiveWins = 0;
                CurrentLevel = 0;
            }
        }

        public ObservableCollection<string> Categories { get; } = new();

        private string _currentWord = string.Empty;
        private List<char> _guessedLetters = new();

        public ObservableCollection<string> WordDisplay { get; } = new();

        private string _wordDisplayText = string.Empty;
        public string WordDisplayText
        {
            get => _wordDisplayText;
            set => SetProperty(ref _wordDisplayText, value);
        }

        private ObservableCollection<LetterButton> _letterButtons = new();
        public ObservableCollection<LetterButton> LetterButtons
        {
            get => _letterButtons;
            set => SetProperty(ref _letterButtons, value);
        }
        private int _wrongGuesses = 0;
        public int WrongGuesses
        {
            get => _wrongGuesses;
            set
            {
                SetProperty(ref _wrongGuesses, value);
                OnPropertyChanged(nameof(HangmanImagePath));
            }
        }

        public int MaxWrong => 6;

        public string HangmanImagePath
        {
            get
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", $"hangman_{_wrongGuesses}.jpeg");
                return File.Exists(path) ? path : string.Empty;
            }
        }

        private double _timeRemaining = 30;
        public double TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                SetProperty(ref _timeRemaining, value);
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }

        public string TimeDisplay => $"{(int)_timeRemaining}s";

        private int _consecutiveWins = 0;
        public int ConsecutiveWins
        {
            get => _consecutiveWins;
            set => SetProperty(ref _consecutiveWins, value);
        }

        private int _currentLevel = 0;
        public int CurrentLevel
        {
            get => _currentLevel;
            set => SetProperty(ref _currentLevel, value);
        }

        public string UserDisplayName => CurrentUser.Username;

        public ImageSource UserAvatar
        {
            get
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CurrentUser.ImagePath);

                if (!File.Exists(path))
                    return null;

                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(path, UriKind.Absolute);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();

                return img;
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _gameActive = false;
        public bool GameActive
        {
            get => _gameActive;
            set => SetProperty(ref _gameActive, value);
        }

        public ICommand NewGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand GuessLetterCommand { get; }
        public ICommand ChangeCategoryCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? OnCancel;

        public GameViewModel(User user)
        {
            CurrentUser = user;

            NewGameCommand = new RelayCommand(_ => StartNewGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame(), _ => _gameActive);
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            GuessLetterCommand = new RelayCommand(p => GuessLetter((char)p!));
            ChangeCategoryCommand = new RelayCommand(p => ChangeCategory(p?.ToString() ?? "All Categories"));
            CancelCommand = new RelayCommand(_ => { _timer.Stop(); OnCancel?.Invoke(); });

            LoadCategories();
            InitLetterButtons();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerTick;
            StartNewGame();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            Categories.Add("All Categories");
            foreach (var c in _gameService.GetCategories())
                Categories.Add(c);
        }

        private void InitLetterButtons()
        {
            LetterButtons.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
                LetterButtons.Add(new LetterButton { Letter = c, IsEnabled = true });
            OnPropertyChanged(nameof(LetterButtons));
        }

        public void StartNewGame()
        {
            _timer.Stop();
            _guessedLetters.Clear();
            WrongGuesses = 0;
            TimeRemaining = 30;
            StatusMessage = string.Empty;
            GameActive = true;

            _currentWord = _gameService.GetRandomWord(_selectedCategory);
            UpdateWordDisplay();
            InitLetterButtons();
            foreach (var btn in LetterButtons)
                btn.IsEnabled = true;
            _timer.Start();
        }

        private void UpdateWordDisplay()
        {
            WordDisplayText = string.Join(" ", _currentWord.Select(c =>
                c == ' ' ? " " : (_guessedLetters.Contains(c) ? c.ToString() : "_")));
        }

        public void GuessLetter(char letter)
        {
            if (!_gameActive) return;
            var btn = LetterButtons.FirstOrDefault(b => b.Letter == letter);
            if (btn != null) btn.IsEnabled = false;

            _guessedLetters.Add(letter);

            if (_currentWord.Contains(letter))
            {
                UpdateWordDisplay();
                if (!WordDisplayText.Contains('_'))
                    WordGuessed();
            }
            else
            {
                WrongGuesses++;
                if (WrongGuesses >= MaxWrong)
                    GameOver();
            }
        }

        private void WordGuessed()
        {
            if (!GameActive) return; 

            _timer.Stop();
            GameActive = false; 

            ConsecutiveWins++;
            CurrentLevel = ConsecutiveWins;
            StatusMessage = $"Corect! Wins consecutive: {ConsecutiveWins}/3";

            if (ConsecutiveWins == 3)
            {
                _statsService.RecordGame(CurrentUser.Username, _selectedCategory, true);
                ConsecutiveWins = 0;

                MessageBox.Show("Ai câștigat jocul! 3 cuvinte ghicite consecutiv!",
                    "Victorie!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var r = MessageBox.Show(
                    "Cuvânt ghicit! Continuăm cu un cuvânt nou?",
                    "Nivel trecut",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (r == MessageBoxResult.Yes)
                {
                    GameActive = true; 
                    StartNewGame();
                }
            }
        }

        private void GameOver()
        {
            _timer.Stop();
            GameActive = false;
            ConsecutiveWins = 0;
            CurrentLevel = 0;

            _statsService.RecordGame(CurrentUser.Username, _selectedCategory, false);
            StatusMessage = $"✖ Ai pierdut! Cuvântul era: {_currentWord}";
            MessageBox.Show($"Ai pierdut! Cuvântul era: {_currentWord}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Error);
            GameActive = true;
            StartNewGame();
        }

        private void TimerTick(object? s, EventArgs e)
        {
            TimeRemaining--;
            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                GameOver();
            }
        }

        private void SaveGame()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Nume salvare:", "Save Game", DateTime.Now.ToString("yyyyMMdd_HHmm"));
            if (string.IsNullOrWhiteSpace(name)) return;
            name = string.Concat(name.Split(Path.GetInvalidFileNameChars()));
            var state = new GameState
            {
                Username = CurrentUser.Username,
                Category = _selectedCategory,
                Word = _currentWord,
                GuessedLetters = new List<char>(_guessedLetters),
                WrongGuesses = WrongGuesses,
                TimeRemaining = TimeRemaining,
                CurrentLevel = CurrentLevel,
                ConsecutiveWins = ConsecutiveWins
            };
            _gameService.SaveGame(state, name);
            MessageBox.Show("Joc salvat!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenGame()
        {
            var saves = _gameService.GetSavedGames(CurrentUser.Username);
            if (saves.Count == 0)
            {
                MessageBox.Show("Nu există jocuri salvate.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var options = string.Join("\n", saves.Select((s, i) => $"{i + 1}. {s}"));
            var input = Microsoft.VisualBasic.Interaction.InputBox($"Alege numărul salvării:\n{options}", "Open Game", "1");
            if (!int.TryParse(input, out int idx) || idx < 1 || idx > saves.Count) return;

            var state = _gameService.LoadGame(CurrentUser.Username, saves[idx - 1]);
            if (state == null) return;

            _timer.Stop();
            _currentWord = state.Word;
            _guessedLetters = state.GuessedLetters;
            WrongGuesses = state.WrongGuesses;
            TimeRemaining = state.TimeRemaining;
            CurrentLevel = state.CurrentLevel;
            ConsecutiveWins = state.ConsecutiveWins;
            _selectedCategory = state.Category;
            OnPropertyChanged(nameof(SelectedCategory));
            GameActive = true;

            InitLetterButtons();
            foreach (var c in _guessedLetters)
            {
                var btn = LetterButtons.FirstOrDefault(b => b.Letter == c);
                if (btn != null) btn.IsEnabled = false;
            }

            UpdateWordDisplay();
            _timer.Start();
        }

        private void ChangeCategory(string category)
        {
            SelectedCategory = category;
            ConsecutiveWins = 0;
            CurrentLevel = 0;
            StartNewGame();
        }
    }
}