using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Transaction_Record.Application.Interfaces;

namespace Transaction_Record.Application.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowMessage(string message, string title = "提示", MessageBoxImage icon = MessageBoxImage.Information)
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, icon);
        }
        
    }
}
