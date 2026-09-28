using Microsoft.UI.Xaml.Controls;
using FinancialApp.Presentation.ViewModels;

namespace FinancialApp.Presentation.Views
{
    public sealed partial class TradeTicketPage : Page
    {
        public TradeTicketPage()
        {
            InitializeComponent();
            DataContext = new TradeTicketViewModel(App.Services.MarketData, App.Services.Orders, "ACC-001");
        }
    }
}
