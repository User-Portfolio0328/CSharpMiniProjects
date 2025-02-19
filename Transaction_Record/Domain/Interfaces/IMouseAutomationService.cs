using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Domain.Interfaces
{
    public interface IMouseAutomationService
    {
        event Action<int> PositionSelected;
        void Dispose();
        Task CopyCraftItemProperty();
        Task MoveMouseToCraftItem();
        Task ClickAlterationOrbOnItemAsync();
        Task ClickAugmentationOrbOnItemAsync();
        Task ClickTransmutationOrbOnItemAsync();
        Task ClickScouringOrbOnItemAsync();
        Task ClickRegalOrbOnItemAsync();
    }
}
