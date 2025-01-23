using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Transaction_Record.Presentation.Views
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : System.Windows.Application
    {
        // 切換Material design主題
        public void ThemeChange(BaseTheme baseTheme)
        {
            var theme = (BundledTheme)System.Windows.Application.Current.Resources.MergedDictionaries[0];
            theme.PrimaryColor = PrimaryColor.Blue;
            theme.BaseTheme = baseTheme;
        }

        // 切換自訂主題
        public void SwitchTheme(string theme)
        {
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri(theme, UriKind.RelativeOrAbsolute)
            };

            // 保留其他 Resource Dictionary
            var existingDicts = System.Windows.Application.Current.Resources.MergedDictionaries.ToList();

            // 清除當前 Resource 並應用新的 Resource Dictionary
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
