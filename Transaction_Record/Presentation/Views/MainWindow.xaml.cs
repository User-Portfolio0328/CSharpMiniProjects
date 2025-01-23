using MaterialDesignThemes.Wpf;
using System.Windows;
using Transaction_Record.Application;
using Transaction_Record.Infrastructure;
using Transaction_Record.Presentation.ViewModels;

namespace Transaction_Record.Presentation.Views
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var repository = new TransactionRepository("transactions.json");
            var service = new TransactionService(repository);
            this.DataContext = new MainViewModel(service);
        }

        // 切換成深色主題
        private void ThemeChange_Checked(object sender, RoutedEventArgs e)
        {
            ((App)System.Windows.Application.Current).ThemeChange(BaseTheme.Dark);
            ((App)System.Windows.Application.Current).SwitchTheme("/Presentation/Resources/DarkTheme.xaml");
        }

        // 切換成淺色主題
        private void ThemeChange_Unchecked(object sender, RoutedEventArgs e)
        {
            ((App)System.Windows.Application.Current).ThemeChange(BaseTheme.Light);
            ((App)System.Windows.Application.Current).SwitchTheme("/Presentation/Resources/LightTheme.xaml");
        }
    }
}