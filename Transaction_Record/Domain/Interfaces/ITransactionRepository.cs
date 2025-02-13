using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        IEnumerable<Transaction> GetAll();
        void Add(Transaction transaction);
        void Delete(int id);
    }
}
