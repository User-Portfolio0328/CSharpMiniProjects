using Aspose.Slides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain.Interfaces;


namespace Transaction_Record.Infrastructure
{
    public class ThemePreferenceRepository : IThemePreferenceRepository
    {
        private const string FILE_PATH = "theme_preference.json";

        public ThemePreferenceRepository()
        {
            // 初始化空檔案（若不存在）
            if (!File.Exists(FILE_PATH))
            {
                File.WriteAllText(FILE_PATH, "[]");
            }
        }

        // 讀取主題
        public string LoadTheme() 
        {
            if (!File.Exists(FILE_PATH))  // 若檔案不存在或無效內容，預設使用淺色主題
            {
                return "Light";
            }

            try 
            {
                var json = File.ReadAllText(FILE_PATH);
                return System.Text.Json.JsonSerializer.Deserialize<string>(json) ?? "Light";
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine($"讀取錯誤:{ex.Message}");
                return "Light";
            }
        }

        // 儲存主題
        public void SaveTheme(string theme)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(theme);
            File.WriteAllText(FILE_PATH, json);
        }
    }
}