using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain;

namespace Transaction_Record.Application.Interfaces
{
    internal interface ITransactionService
    {
        void AddTransaction(Transaction transaction); // 新增交易
        void DeleteTransaction(int id); // 刪除交易
        IEnumerable<Transaction> GetTransactions(); // 取得所有交易
        decimal GetTotalAmount(string type); // 計算收入或支出總金額
        decimal ComputePnL(); //計算損益
    }
}
