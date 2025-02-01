using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Transaction_Record.Infrastructure;

namespace Transaction_Record.Presentation
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : System.Windows.Application
    {
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 根據最後的設定來套用主題
            this.SwitchTheme();
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
            var themeRepository = new ThemePreferenceRepository();
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
