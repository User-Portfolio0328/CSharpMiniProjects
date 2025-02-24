using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain.Interfaces;

namespace Transaction_Record.Presentation.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected readonly IMouseAutomationService _mouseAutomationService;

        public BaseViewModel(IMouseAutomationService mouseAutomationService = null) 
        { 
            this._mouseAutomationService = mouseAutomationService;
            if (this._mouseAutomationService != null)
            {
                this._mouseAutomationService.PositionSelected += this.OnPositionSelected;
            }
        }  
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Dispose() 
        {
            if (this._mouseAutomationService != null)
            {
                this._mouseAutomationService.PositionSelected -= OnPositionSelected;
            }
        }
        protected virtual void OnPositionSelected(int step) { }
    }
}
