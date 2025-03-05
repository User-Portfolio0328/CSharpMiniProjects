using Aspose.Slides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Infrastructure.Services;


namespace Transaction_Record.Infrastructure
{
    public class ThemePreferenceRepository : IThemePreferenceRepository
    {
        private readonly DatabaseService _databaseService;

        public ThemePreferenceRepository(DatabaseService databaseService)
        {
            this._databaseService = databaseService;
        }

        // 讀取主題
        public string LoadTheme() 
        {
            string result = "";

            using (var connection = this._databaseService.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Theme FROM ThemePreferences LIMIT 1;";
                    result = command.ExecuteScalar()?.ToString();
                }
            }
            return result ?? "Light";
        }

        // 儲存主題
        public void SaveTheme(string theme)
        {
            using (var connection = this._databaseService.GetConnection())
            {
                using (var deleteCommand = connection.CreateCommand())
                {
                    deleteCommand.CommandText = "DELETE FROM ThemePreferences;";
                    deleteCommand.ExecuteNonQuery();
                }

                using (var insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText = "INSERT INTO ThemePreferences (Theme) VALUES (@Theme);";
                    insertCommand.Parameters.AddWithValue("@Theme", theme);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }
    }
}