using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain.Enums;

namespace Transaction_Record.Application.Interfaces
{
    public interface ISelectionDataProviderService
    {
        ObservableCollection<string> GetTransactionType();
        ObservableCollection<AffixSlot> GetAffixType();
        ObservableCollection<string> GetAffixTier();
        ObservableCollection<RollType> GetRollType();
    }
}
