using Aspose.Slides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Transaction_Record.Infrastructure
{
    public class ThemePreferenceRepository
    {
        private const string FilePath = "theme_preference.json";

        // 讀取主題
        public string LoadTheme() 
        {
            if (!File.Exists(FilePath))  // 若檔案不存在或無效內容，預設使用淺色主題
            {
                return "Light";
            }

            try 
            {
                var json = File.ReadAllText(FilePath);
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
            File.WriteAllText(FilePath, json);
        }
    }
}