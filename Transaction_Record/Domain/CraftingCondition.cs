using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain.Enums;

namespace Transaction_Record.Domain
{
    public class CraftingCondition
    {
        public int Id { get; set; } // 唯一標示
        public string Keyword { get; set; } // 詞綴關鍵字
        public AffixSlot AffixType { get; set; } // 詞綴類別(第幾個前綴或後綴) 
        public string AffixTier { get; set; } // 詞綴階級

        // 驗證方法
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(this.Keyword))
            {
                return false;
            }

            if (string.IsNullOrEmpty(AffixTier)) 
            {
                return false;
            }

            return true;
        }
    }
}
