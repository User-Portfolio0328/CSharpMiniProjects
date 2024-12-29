using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Domain
{
    public class Transaction
    {
        public int Id { get; set; } // 唯一標示
        public string Type  { get; set; } // 收入或支出
        public string Category { get; set; } // 分類，例如：飲食、交通
        public string Note { get; set; } // 備註
        public decimal Amount { get; set; } // 金額
        public DateTime Date { get; set; } // 日期


        // 驗證方法
        public bool IsValid() 
        {
            if (string.IsNullOrWhiteSpace(Type)) 
                return false;

            if (string.IsNullOrWhiteSpace(Category)) 
                return false;

            if (this.Amount <= 0) 
                return false;

            return true;
        }


    }
}
