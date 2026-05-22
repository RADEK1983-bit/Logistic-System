using System.Windows;
using LogisticsSystem.UI.ViewModels;

namespace LogisticsSystem.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.SaveDataToFile();
        }
    }
}