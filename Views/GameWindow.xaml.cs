using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hangman.Models;
using Hangman.ViewModels;

namespace Hangman.Views
{
    public partial class GameWindow : Window
    {
        private readonly GameViewModel _vm;

        public GameWindow(User user)
        {
            InitializeComponent();
            _vm = new GameViewModel(user);
            DataContext = _vm;
            _vm.OnCancel += () => { this.Close(); };

            BuildCategoriesMenu();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is GameViewModel vm)
            {
                vm.StopTimer(); 
            }
        }

        private void BuildCategoriesMenu()
        {
            CategoriesMenu.Items.Clear();
            foreach (var cat in _vm.Categories)
            {
                var item = new MenuItem { Header = cat };
                item.Click += (_, _) => _vm.ChangeCategoryCommand.Execute(cat);
                CategoriesMenu.Items.Add(item);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N: _vm.NewGameCommand.Execute(null); break;
                    case Key.O: _vm.OpenGameCommand.Execute(null); break;
                    case Key.S: _vm.SaveGameCommand.Execute(null); break;
                    case Key.Q: _vm.CancelCommand.Execute(null); break;
                }
                return;
            }

            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char letter = (char)('A' + (e.Key - Key.A));
                _vm.GuessLetterCommand.Execute(letter);
            }
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            new StatisticsWindow().ShowDialog();
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }
    }
}
