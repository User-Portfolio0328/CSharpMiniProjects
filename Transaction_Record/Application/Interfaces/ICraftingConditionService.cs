using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Application.Interfaces
{
    public interface ICraftingConditionService
    {
        bool IsAffixMatching(string itemProperty);
        bool NeedUseAugmentationOrb(string itemProperties);
        string ExtractValueByKeyword(string itemProperties, string regex);

    }
}
