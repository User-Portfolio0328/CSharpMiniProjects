using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        IEnumerable<Transaction> LoadTransaction();
        void AddTransaction(Transaction transaction);
        void DeleteTransaction(int id);
    }
}
