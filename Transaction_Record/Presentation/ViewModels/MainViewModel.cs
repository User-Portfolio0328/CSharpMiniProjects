using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Transaction_Record.Application;
using Transaction_Record.Domain;
using Transaction_Record.Infrastructure;
using Transaction_Record.Presentation.Commands;

namespace Transaction_Record.Presentation.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        #region Properties
        private readonly TransactionService _service;
        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();
        public decimal TotalIncome => this._service.GetTotalAmount("收入");
        public decimal TotalExpense => this._service.GetTotalAmount("支出");
        public decimal ProfitAndLoss => this._service.ComputePnL();

        public ObservableCollection<string> Types { get; set; }
            = new ObservableCollection<string> { "收入", "支出" };

        private string _type;
        public string Type
        {
            get => this._type;
            set
            {
                this._type = value;
                this.OnPropertyChanged(nameof(this.Type));
            }
        }

        private string _category;
        public string Category
        {
            get => this._category;
            set
            {
                this._category = value;
                this.OnPropertyChanged(nameof(this.Category));
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => this._amount;
            set
            {
                this._amount = value;
                this.OnPropertyChanged(nameof(this.Amount));
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => this._date;
            set
            {
                this._date = value;
                this.OnPropertyChanged(nameof(this.Date));
            }
        }

        private Transaction _selectedTransactions;
        public Transaction SelectedTransaction
        {
            get => this._selectedTransactions;
            set
            {
                this._selectedTransactions = value;
                this.OnPropertyChanged(nameof(this.SelectedTransaction));
            }
        }

        private readonly ThemePreferenceRepository _themeRepository;
        private string _currentTheme;
        public string CurrentTheme
        {
            get => this._currentTheme;
            set
            {
                this._currentTheme = value;
                this.OnPropertyChanged(nameof(this.CurrentTheme));
            }
        }

        #endregion

        #region Commands
        public ICommand AddTransectionCommand { get; }
        public ICommand DeleteTransectionCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        #endregion

        public MainViewModel(TransactionService service)
        {
            this.AddTransectionCommand = new RelayCommand(this.AddTransaction);
            this.DeleteTransectionCommand = new RelayCommand(this.DeleteTransaction);
            this.ChangeThemeCommand = new RelayCommand(this.ChangeTheme);

            this.Date = DateTime.Now;
            this._service = service;
            this._themeRepository = new ThemePreferenceRepository();
            this.CurrentTheme = this._themeRepository.LoadTheme();

            this.LoadTransactions();
        }

        // 載入交易
        private void LoadTransactions()
        {
            this.Transactions.Clear();
            foreach (var transaction in this._service.GetTransactions())
            {
                this.Transactions.Add(transaction);
            }
        }

        // 新增交易
        private void AddTransaction(object parameter)
        {
            try
            {
                var transaction = (new Transaction
                {
                    Type = this.Type,
                    Category = this.Category,
                    Amount = this.Amount,
                    Date = this.Date
                });

                this._service.AddTransaction(transaction);
                this.LoadTransactions();
                this.UpdateTotals();

                this.Type = transaction.Type;
                this.Category = string.Empty;
                this.Amount = 0;
                this.Date = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"新增交易失敗：{ex.Message}",
                    "錯誤",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 刪除交易
        private void DeleteTransaction(object parameter)
        {
            if (parameter is Transaction transaction)
            {
                this._service.DeleteTransaction(transaction.Id);
                this.LoadTransactions();
                this.UpdateTotals();
            }
            else
            {
                MessageBox.Show(
                    "請選擇要刪除的交易",
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 切換主題
        private void ChangeTheme(object parameter)
        {
            if (parameter is string theme)
            {
                this.CurrentTheme = theme;
                this._themeRepository.SaveTheme(theme);

                ((App)System.Windows.Application.Current).SwitchTheme();                
            }
        }

        // 更新計算面板數字
        private void UpdateTotals()
        {
            this.OnPropertyChanged(nameof(this.TotalIncome));
            this.OnPropertyChanged(nameof(this.TotalExpense));
            this.OnPropertyChanged(nameof(this.ProfitAndLoss));
        }
    }
}
