﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain;
using Transaction_Record.Infrastructure;

namespace Transaction_Record.Application
{
    public class TransactionService : ITransactionService
    {
        private readonly TransactionRepository _repository;

        public TransactionService(TransactionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // 新增交易
        public void AddTransaction(Transaction transaction)
        {
            if (transaction == null || !transaction.IsValid())
            {
                throw new ArgumentException("Invalid transaction");
            }

            _repository.Add(transaction);
        }

        // 刪除交易
        public void DeleteTransaction(int id)
        {
            _repository.Delete(id);
        }

        // 獲取所有交易
        public IEnumerable<Transaction> GetTransactions()
        {
            return _repository.GetAll();
        }

        // 計算收入或支出總金額
        public decimal GetTotalAmount(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty");
            }

            return _repository
                .GetAll()
                .Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Amount);
        }
    }
}