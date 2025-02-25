using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Transaction_Record.Application.Interfaces
{
    public interface IMessageBoxService
    {
        void ShowMessage(string message, string title = "提示", MessageBoxImage icon = MessageBoxImage.Information);
    }
}
