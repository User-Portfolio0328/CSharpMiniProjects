using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Infrastructure.Services
{
    public class DatabaseService
    {
        private const string FilePath = "Database.db";
        private const string ConnectionString = "Data Source=" + FilePath + ";Version=3";

        public DatabaseService()
        {
            this.InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(FilePath))
            {
                SQLiteConnection.CreateFile(FilePath);

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    try
                    {
                        connection.Open();

                        string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS CraftingConditions (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Keyword TEXT NOT NULL,
                            AffixType TEXT NOT NULL,
                            AffixTier TEXT NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Transactions (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Type TEXT NOT NULL,
                            Category TEXT NOT NULL,
                            Note TEXT,
                            Amount REAL NOT NULL,
                            Date TEXT NOT NULL
                        );

                         CREATE TABLE IF NOT EXISTS ThemePreferences (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Theme TEXT NOT NULL
                        );";

                        using (var command = new SQLiteCommand(createTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"資料表創建失敗: {ex.Message}");
                    }
                }
            }
        }

        public SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

    }
}
