using Aspose.Slides.Theme;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Transaction_Record.Application;
using Transaction_Record.Domain;
using Transaction_Record.Infrastructure;

namespace Transaction_Record.Presentation
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TransactionService _service;

        public MainWindow()
        {
            InitializeComponent();
            var repository = new TransactionRepository("transactions.json");
            this._service = new TransactionService(repository);

            this.LoadTransactions();
        }

        private void LoadTransactions()
        {
            var transactions = this._service.GetTransactions();
            TransactionGrid.ItemsSource = transactions.ToList();
            TotalIncome.Text = this._service.GetTotalAmount(TypeComboBox.Text).ToString("C");
            TotalExpense.Text = this._service.GetTotalAmount(TypeComboBox.Text).ToString("C");
            ProfitAndLoss.Text = this._service.ComputePnL().ToString("C");
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var transaction = new Transaction
                {
                    Category = CategoryTextBox.Text,
                    Amount = decimal.Parse(AmountTextBox.Text),
                    Type = TypeComboBox.Text,
                    Date = (DateTime)DatePicker.SelectedDate
                };

                this._service.AddTransaction(transaction);
                this.LoadTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"新增交易失敗：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionGrid.SelectedItem is Transaction selectedTransaction)
            {
                this._service.DeleteTransaction(selectedTransaction.Id);
                this.LoadTransactions();
            }
            else
            {
                MessageBox.Show("請選擇要刪除的交易", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ThemeChange_Checked(object sender, RoutedEventArgs e)
        {
            var theme = (BundledTheme)System.Windows.Application.Current.Resources.MergedDictionaries[0];
            theme.PrimaryColor = PrimaryColor.DeepPurple;
            theme.BaseTheme = BaseTheme.Dark;
            this.SwitchTheme("/Presentation/Resources/DarkTheme.xaml");
        }

        private void ThemeChange_Unchecked(object sender, RoutedEventArgs e)
        {
            var theme = (BundledTheme)System.Windows.Application.Current.Resources.MergedDictionaries[0];
            theme.PrimaryColor = PrimaryColor.Blue;
            theme.BaseTheme = BaseTheme.Light;
            this.SwitchTheme("/Presentation/Resources/LightTheme.xaml");
        }

        // 切換成指定的主題
        private void SwitchTheme(string theme)
        {
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri(theme, UriKind.RelativeOrAbsolute)
            };

            // 保留其他 Resource Dictionary
            var existingDicts = System.Windows.Application.Current.Resources.MergedDictionaries.ToList();

            // 清除當前資源並應用新的 Resource Dictionary
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();

            // 先將已經存在的其他 Resource Dictionary 加回來
            foreach (var dict in existingDicts)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
            }

            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
    }
}
