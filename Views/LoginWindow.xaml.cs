using System.Windows;
using Hangman.Models;
using Hangman.ViewModels;

namespace Hangman.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            DataContext = _vm;
            _vm.OnPlay += OpenGame;
        }

        private void OpenGame(User user)
        {
            var gameWin = new GameWindow(user);
            gameWin.Show();
            // Nu inchidem LoginWindow, doar il ascundem
            this.Hide();
            gameWin.Closed += (_, _) => this.Show();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
