using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Hangman.Commands;
using Hangman.Models;
using Hangman.Services;
using Microsoft.Win32;

namespace Hangman.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserService _userService = new();

        public ObservableCollection<User> Users { get; } = new();

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(IsUserSelected));
                OnPropertyChanged(nameof(SelectedUserImage));
                OnPropertyChanged(nameof(AvatarPreview));
            }
        }

        public bool IsUserSelected => _selectedUser != null;

        public string SelectedUserImage => _selectedUser?.ImagePath is { Length: > 0 } p
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p)
            : string.Empty;
        public string AvatarPreview
        {
            get
            {
                if (_selectedUser?.ImagePath is { Length: > 0 } p1)
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p1);
              
                if (!string.IsNullOrWhiteSpace(NewUserImagePath))
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NewUserImagePath);

                return string.Empty;
            }
        }
        private string _newUsername = string.Empty;
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _newUserImagePath = string.Empty;
        public string NewUserImagePath
        {
            get => _newUserImagePath;
            set
            {
                if (SetProperty(ref _newUserImagePath, value))
                    OnPropertyChanged(nameof(AvatarPreview));
            }
        }
        public string NewUserImagePreview
            => string.IsNullOrWhiteSpace(NewUserImagePath)
                ? string.Empty
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NewUserImagePath);
        public ObservableCollection<string> PredefinedAvatars { get; } = new();
        private int _avatarIndex = 0;

        public ICommand PlayCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand NewUserCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand NextAvatarCommand { get; }
        public ICommand PrevAvatarCommand { get; }

        public event Action<User>? OnPlay;
        public event Action? OnCancel;

        public LoginViewModel()
        {
            LoadUsers();
            LoadAvatars();

            PlayCommand = new RelayCommand(_ => Play(), _ => IsUserSelected);
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => IsUserSelected);
            NewUserCommand = new RelayCommand(_ => CreateUser());
            BrowseImageCommand = new RelayCommand(_ => BrowseImage());
            NextAvatarCommand = new RelayCommand(_ => NextAvatar());
            PrevAvatarCommand = new RelayCommand(_ => PrevAvatar());
        }

        private void LoadUsers()
        {
            Users.Clear();
            foreach (var u in _userService.LoadUsers())
                Users.Add(u);
        }

        private void LoadAvatars()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "avatars");
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.GetFiles(dir, "*.png").Concat(Directory.GetFiles(dir, "*.gif")))
                PredefinedAvatars.Add(Path.Combine("Images", "avatars", Path.GetFileName(f)));
            if (PredefinedAvatars.Count > 0)
                NewUserImagePath = PredefinedAvatars[0];
        }

        private void NextAvatar()
        {
            if (PredefinedAvatars.Count == 0) return;
            _avatarIndex = (_avatarIndex + 1) % PredefinedAvatars.Count;
            NewUserImagePath = PredefinedAvatars[_avatarIndex];
        }

        private void PrevAvatar()
        {
            if (PredefinedAvatars.Count == 0) return;
            _avatarIndex = (_avatarIndex - 1 + PredefinedAvatars.Count) % PredefinedAvatars.Count;
            NewUserImagePath = PredefinedAvatars[_avatarIndex];
        }

        private void BrowseImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.gif",
                Title = "Select avatar image"
            };
            if (dlg.ShowDialog() == true)
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var selected = dlg.FileName;
                if (selected.StartsWith(basePath))
                    NewUserImagePath = Path.GetRelativePath(basePath, selected);
                else
                    NewUserImagePath = selected;
            }
        }

        private void CreateUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || NewUsername.Contains(' '))
            {
                MessageBox.Show("Username-ul trebuie sa fie un singur cuvant!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_userService.UserExists(NewUsername))
            {
                MessageBox.Show("Username-ul exista deja!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var user = new User { Username = NewUsername, ImagePath = NewUserImagePath };
            _userService.AddUser(user);
            NewUsername = string.Empty;
            LoadUsers();
        }

        private void DeleteUser()
        {
            if (_selectedUser == null) return;
            var result = MessageBox.Show($"Stergi utilizatorul {_selectedUser.Username}?", "Confirmare",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var statsService = new StatisticsService();
            statsService.DeleteUser(_selectedUser.Username);
            _userService.DeleteUser(_selectedUser.Username);
            SelectedUser = null;
            LoadUsers();
        }

        private void Play()
        {
            if (_selectedUser != null)
                OnPlay?.Invoke(_selectedUser);
        }
    }
}