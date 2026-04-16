using System.Windows;
using Hangman.ViewModels;

namespace Hangman.Views
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
