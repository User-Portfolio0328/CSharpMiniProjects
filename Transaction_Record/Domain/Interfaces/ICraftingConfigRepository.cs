using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Domain.Interfaces
{
    public interface ICraftingConfigRepository
    {
        ObservableCollection<CraftingCondition> LoadCondition();
        void AddCondition(CraftingCondition craftingCondition);
        void DeleteCondition(int id);
    }
}
