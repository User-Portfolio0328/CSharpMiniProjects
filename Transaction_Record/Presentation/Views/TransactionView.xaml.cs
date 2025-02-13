using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using Transaction_Record.Application.Services;
using Transaction_Record.Infrastructure;
using Transaction_Record.Presentation.ViewModels;

namespace Transaction_Record.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class TransactionView
    {
        public TransactionView()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<TransactionViewModel>();
            this.DataContext = viewModel;
        }
    }
}
