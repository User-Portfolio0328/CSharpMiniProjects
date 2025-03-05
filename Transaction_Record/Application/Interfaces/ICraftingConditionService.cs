using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Enums;

namespace Transaction_Record.Application.Interfaces
{
    public interface ICraftingConditionService
    {
        void AddCondition(CraftingCondition craftingCondition);
        void DeleteCondition(int id);
        ObservableCollection<CraftingCondition> LoadCondition();
        Task ExecuteMouseClickAndCompare(RollType rollType);
    }
}
