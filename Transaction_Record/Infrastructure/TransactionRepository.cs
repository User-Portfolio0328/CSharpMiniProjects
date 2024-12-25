using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Transaction_Record.Domain;

namespace Transaction_Record.Infrastructure
{
    public class TransactionRepository
    {
        private readonly string _filePath;

        public TransactionRepository(string filePath)
        {
            this._filePath = filePath;

            // 初始化空檔案（若不存在）
            if (!File.Exists(this._filePath))
            {
                File.WriteAllText(this._filePath, "[]");
            }
        }

        // 獲取所有交易
        public IEnumerable<Transaction> GetAll()
        {
            var json = File.ReadAllText(this._filePath);
            return JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
        }

        // 新增交易
        public void Add(Transaction transaction)
        {
            var transactions = GetAll().ToList();
            transaction.Id = transactions.Count > 0 ? transactions.Max(t => t.Id) + 1 : 1;
            transactions.Add(transaction);
            SaveAll(transactions);
        }

        // 刪除交易
        public void Delete(int id)
        {
            var transactions = GetAll().ToList();
            var transaction = transactions.FirstOrDefault(t => t.Id == id);

            if (transaction != null)
            {
                transactions.Remove(transaction);
                SaveAll(transactions);
            }
        }

        // 儲存所有交易
        private void SaveAll(IEnumerable<Transaction> transactions)
        {
            var json = JsonConvert.SerializeObject(transactions, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(this._filePath, json);
        }
    }
}
