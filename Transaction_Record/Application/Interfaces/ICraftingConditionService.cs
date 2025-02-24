using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain.Enums;

namespace Transaction_Record.Application.Interfaces
{
    public interface ICraftingConditionService
    {
        Task ExecuteMouseClickAndCompare(RollType rollType);
    }
}
