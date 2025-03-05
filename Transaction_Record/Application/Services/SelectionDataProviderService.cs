using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Domain.Enums;

namespace Transaction_Record.Application.Services
{
    public class SelectionDataProviderService : ISelectionDataProviderService
    {
        public ObservableCollection<string> GetTransactionType()
        {
            return new ObservableCollection<string> { "收入", "支出" };
        }
        public ObservableCollection<AffixSlot> GetAffixType()
        {
            return new ObservableCollection<AffixSlot>((AffixSlot[])Enum.GetValues(typeof(AffixSlot)));
        }
        public ObservableCollection<string> GetAffixTier()
        {
            return new ObservableCollection<string>
            {
                "T1",
                "T2",
                "T3",
                "T4",
                "T5",
                "T6",
                "T7",
                "T8",
                "T9",
                "T10",
                "T11",
                "T12",
                "T13",
                "T14",
                "T15",
                "T16",
                "T17",
                "T18",
                "T19"
            };
        }

        public ObservableCollection<RollType> GetRollType()
        {
            return new ObservableCollection<RollType>((RollType[])Enum.GetValues(typeof(RollType)));
        }
    }
}
