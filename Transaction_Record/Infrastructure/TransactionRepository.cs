using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Xml;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Infrastructure.Services;

namespace Transaction_Record.Infrastructure
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DatabaseService _databaseService;

        public TransactionRepository(DatabaseService databaseService)
        {
            this._databaseService = databaseService;
        }

        // 獲取所有交易
        public IEnumerable<Transaction> LoadTransaction()
        {
            var transactions = new List<Transaction>();

            using (var connection = this._databaseService.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Transactions ORDER BY Date DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                Id = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                Category = reader.GetString(2),
                                Note = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Amount = reader.GetDecimal(4),
                                Date = DateTime.Parse(reader.GetString(5))
                            });
                        }
                    }
                }
            }

            return transactions;
        }

        // 新增交易
        public void AddTransaction(Transaction transaction)
        {
            if (!transaction.IsValid()) 
            {
                throw new InvalidOperationException("Invalid transaction");
            }

            using (var connection = this._databaseService.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        @"
                    INSERT INTO Transactions (Type, Category, Note, Amount, Date)
                    VALUES (@Type, @Category, @Note, @Amount, @Date);
                    ";

                    command.Parameters.AddWithValue("@Type", transaction.Type);
                    command.Parameters.AddWithValue("@Category", transaction.Category);
                    command.Parameters.AddWithValue("@Note", string.IsNullOrEmpty(transaction.Note) ? (object)DBNull.Value : transaction.Note);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@Date", transaction.Date.ToString());

                    command.ExecuteNonQuery();
                }
            }
        }

        // 刪除交易
        public void DeleteTransaction(int id)
        {
            using (var connection = this._databaseService.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Transactions WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
