using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Record.Domain.Interfaces
{
    internal interface IThemePreferenceRepository
    {
        string LoadTheme();
        void SaveTheme(string theme);
    }
}
