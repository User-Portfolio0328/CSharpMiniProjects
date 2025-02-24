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
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Application層
            services.AddSingleton<ICraftingConditionService, CraftingConditionService>();
            services.AddSingleton<ITransactionRepository>(provider => new TransactionRepository());
            services.AddSingleton<ITransactionService, TransactionService>();

            // Domain層
            services.AddSingleton<ObservableCollection<CraftingCondition>>();
            services.AddSingleton<ObservableCollection<Transaction>>();

            // Infrastructure層
            
            services.AddSingleton<IThemePreferenceRepository>(provider => new ThemePreferenceRepository());// 註冊需要的服務
            services.AddSingleton<ICraftingConfigRepository, CraftingConfigRepository>();
            services.AddSingleton<ITransactionRepository, TransactionRepository>();
            services.AddSingleton<IThemePreferenceRepository, ThemePreferenceRepository>();
            services.AddSingleton<IMouseAutomationService, MouseAutomationService>();

            // ViewModel層
            services.AddTransient<CraftingConfigViewModel>();
            services.AddTransient<TransactionViewModel>();
            services.AddTransient<MainViewModel>();

            // View層
            services.AddTransient<MainWindow>();
            services.AddTransient<CraftingConfigView>();
            services.AddTransient<TransactionView>();


            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
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

            if (ServiceProvider.GetService<ICraftingConditionService>() is IDisposable disposable) 
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        // 切換Material design主題
        public void ThemeChange(BaseTheme baseTheme)
        {
            var theme = (BundledTheme)System.Windows.Application.Current.Resources.MergedDictionaries[0];
            theme.PrimaryColor = PrimaryColor.Blue;
            theme.BaseTheme = baseTheme;
        }

        // 切換自訂主題
        public void SwitchTheme()
        {
            // 讀取儲存的主題
            var themeRepository = App.ServiceProvider.GetRequiredService<IThemePreferenceRepository>();
            string saveTheme = themeRepository.LoadTheme();

            // 根據讀取的主題來設定資源路徑,並切換主題樣式
            string resourcePath;
            if (saveTheme == "Dark")
            {
                resourcePath = "/Presentation/Resources/DarkTheme.xaml";
                this.ThemeChange(BaseTheme.Dark);
            }
            else
            {
                resourcePath = "/Presentation/Resources/LightTheme.xaml";
                this.ThemeChange(BaseTheme.Light);
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
