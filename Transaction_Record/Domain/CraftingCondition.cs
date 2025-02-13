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
        public string Keyword { get; set; }
        public AffixSlot AffixType { get; set; }
        public string AffixTier { get; set; }
    }
}
