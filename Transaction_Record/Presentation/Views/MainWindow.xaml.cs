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
    }
}