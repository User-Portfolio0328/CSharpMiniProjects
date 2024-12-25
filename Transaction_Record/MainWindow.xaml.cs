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

namespace Transaction_Record
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
            _service = new TransactionService(repository);

            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var transactions = _service.GetTransactions();
            TransactionGrid.ItemsSource = transactions.ToList();
            TotalIncome.Text = _service.GetTotalAmount("收入").ToString("C");
            TotalExpense.Text = _service.GetTotalAmount("支出").ToString("C");
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var transaction = new Transaction
                {
                    Category = CategoryInput.Text,
                    Amount = decimal.Parse(AmountInput.Text),
                    Type = (TypeInput.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Date = DateTime.Now
                };

                _service.AddTransaction(transaction);
                LoadTransactions();
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
                _service.DeleteTransaction(selectedTransaction.Id);
                LoadTransactions();
            }
            else
            {
                MessageBox.Show("請選擇要刪除的交易", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
