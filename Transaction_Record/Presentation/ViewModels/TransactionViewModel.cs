﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Transaction_Record.Application.Services;
using Transaction_Record.Domain;
using Transaction_Record.Infrastructure;
using Transaction_Record.Presentation.Commands;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Domain.Interfaces;

namespace Transaction_Record.Presentation.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        #region Properties
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

        private readonly IThemePreferenceRepository _themeRepository;
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

        private readonly ITransactionService _service;
        public ObservableCollection<Transaction> Transactions { get; set; }
        public decimal TotalIncome => this._service.GetTotalAmount("收入");
        public decimal TotalExpense => this._service.GetTotalAmount("支出");
        public decimal ProfitAndLoss => this._service.ComputePnL();
        public ObservableCollection<string> Types { get; set; }
        private readonly ISelectionDataProviderService _selectionDataProviderService;
        private readonly IMessageBoxService _messageBoxService;


        #endregion

        #region Commands
        public ICommand AddTransectionCommand => new RelayCommand(this.AddTransaction);
        public ICommand DeleteTransectionCommand => new RelayCommand(this.DeleteTransaction);
        public ICommand ChangeThemeCommand => new RelayCommand(this.ChangeTheme);
        #endregion
        
        public TransactionViewModel(
            ObservableCollection<Transaction> transactions,
            ITransactionService service, 
            IThemePreferenceRepository themePreferenceRepository,
            ISelectionDataProviderService selectionDataProviderService,
            IMessageBoxService messageBoxService)
        {
            this._service = service;
            this._themeRepository = themePreferenceRepository;
            this.Transactions = transactions;
            this._selectionDataProviderService = selectionDataProviderService;
            this.Types = selectionDataProviderService.GetTransactionType();
            this.Date = DateTime.Now;
            this.CurrentTheme = this._themeRepository.LoadTheme();
            this._messageBoxService = messageBoxService;
            this.RefreshTransactions();
        }

        // 載入交易
        private void RefreshTransactions()
        {
            var transactions = this._service.GetTransactions();
            this.Transactions.Clear();
            foreach (var transaction in transactions)
            {
                this.Transactions.Add(transaction);
            }
        }

        // 新增交易
        private void AddTransaction(object parameter)
        {
            try
            {
                var transaction = new Transaction
                {
                    Type = this.Type,
                    Category = this.Category,
                    Amount = this.Amount,
                    Date = this.Date
                };

                this._service.AddTransaction(transaction);
                this.RefreshTransactions();
                this.UpdateTotals();

                this.Type = transaction.Type;
                this.Category = string.Empty;
                this.Amount = 0;
                this.Date = DateTime.Now;
            }
            catch (Exception ex)
            {
                this._messageBoxService.ShowMessage($"新增交易失敗：{ex.Message}", "錯誤", MessageBoxImage.Error);
            }
        }

        // 刪除交易
        private void DeleteTransaction(object parameter)
        {
            if (parameter is Transaction transaction)
            {
                this._service.DeleteTransaction(transaction.Id);
                this.RefreshTransactions();
                this.UpdateTotals();
            }
            else
            {
                this._messageBoxService.ShowMessage("請選擇要刪除的交易", "提示");
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
