using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Transaction_Record.Application.Services;
using Transaction_Record.Domain;
using Transaction_Record.Infrastructure;
using Transaction_Record.Infrastructure.Services;
using Transaction_Record.Presentation.Commands;
using Transaction_Record.Presentation.Views;

namespace Transaction_Record.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Properties
        public ObservableCollection<TabItem> Tabs { get; set; } // 標籤集合

        private TabItem _currentTab;
        public TabItem CurrentTab
        {
            get => _currentTab;
            set
            {
                _currentTab = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public ICommand AddTabCommand => new RelayCommand(this.AddTab);
        public ICommand RemoveTabCommand => new RelayCommand(this.RemoveTab);

        public MainViewModel(TransactionView transactionView, CraftingConfigView craftingConfigView)
        {
            Tabs = new ObservableCollection<TabItem>
        {
            new TabItem { Name = "交易記錄", Page = transactionView },
            new TabItem { Name = "做裝設定", Page = craftingConfigView }
        };
            CurrentTab = Tabs[0];
        }

        private void AddTab(object parameter) 
        {
            this.Tabs.Add(new TabItem { Name = "GG"});
        }

        private void RemoveTab(object parameter) 
        {
            if (Tabs.Any()) 
            {
                Tabs.RemoveAt(Tabs.Count - 1);
            }
        }
        
    }
}
