using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;
using System.Windows;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Application.Services;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Infrastructure;
using Transaction_Record.Infrastructure.Services;
using Transaction_Record.Presentation.ViewModels;
using Transaction_Record.Presentation.Views;

namespace Transaction_Record.Presentation
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static IServiceProvider _serviceProvider { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Application層
            services.AddSingleton<ICraftingConditionService, CraftingConditionService>();
            services.AddSingleton<ITransactionRepository, TransactionRepository>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<IMessageBoxService, MessageBoxService>();
            services.AddSingleton<ISelectionDataProviderService, SelectionDataProviderService>();

            // Domain層
            services.AddSingleton<ObservableCollection<CraftingCondition>>();
            services.AddSingleton<ObservableCollection<Transaction>>();

            // Infrastructure層
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<IThemePreferenceRepository>(provider => 
            {
                var databaseService = provider.GetRequiredService<DatabaseService>();
                return new ThemePreferenceRepository(databaseService);
            });
            services.AddSingleton<ICraftingConfigRepository, CraftingConfigRepository>();
            services.AddSingleton<ITransactionRepository, TransactionRepository>();
            services.AddSingleton<IThemePreferenceRepository, ThemePreferenceRepository>();
            services.AddSingleton<IMouseAutomationService, MouseAutomationService>();
            services.AddTransient<CraftingConfigRepository>();

            // ViewModel層
            services.AddTransient<CraftingConfigViewModel>();
            services.AddTransient<TransactionViewModel>();
            services.AddTransient<MainViewModel>();

            // View層
            services.AddTransient<MainWindow>();
            services.AddTransient<CraftingConfigView>();
            services.AddTransient<TransactionView>();


            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);

            // 根據最後的設定來套用主題
            this.SwitchTheme();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MainWindow.DataContext is MouseAutomationService vm)
            {
                vm.Dispose();
            }

            if (_serviceProvider.GetService<ICraftingConditionService>() is IDisposable disposable) 
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        // 切換Material design主題
        public void ChangeTheme(BaseTheme baseTheme)
        {
            var theme = (BundledTheme)System.Windows.Application.Current.Resources.MergedDictionaries[0];
            theme.PrimaryColor = PrimaryColor.Blue;
            theme.BaseTheme = baseTheme;
        }

        // 切換自訂主題
        public void SwitchTheme()
        {
            // 讀取儲存的主題
            var themeRepository = App._serviceProvider.GetRequiredService<IThemePreferenceRepository>();
            string saveTheme = themeRepository.LoadTheme();

            // 根據讀取的主題來設定資源路徑,並切換主題樣式
            string resourcePath;
            if (saveTheme == "Dark")
            {
                resourcePath = "/Presentation/Resources/DarkTheme.xaml";
                this.ChangeTheme(BaseTheme.Dark);
            }
            else
            {
                resourcePath = "/Presentation/Resources/LightTheme.xaml";
                this.ChangeTheme(BaseTheme.Light);
            }

            // 建立Resource Dictionary, 根據指定的資源路徑載入對應樣式
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri(resourcePath, UriKind.RelativeOrAbsolute)
            };

            // 保留其他 Resource Dictionary
            var existingDicts = Resources.MergedDictionaries.ToList();

            // 清除當前 Resource 並應用新的 Resource Dictionary
            Resources.MergedDictionaries.Clear();

            // 先將已經存在的其他 Resource Dictionary 加回來
            foreach (var dict in existingDicts)
            {
                Resources.MergedDictionaries.Add(dict);
            }

            Resources.MergedDictionaries.Add(resourceDict);
        }
    }
}
