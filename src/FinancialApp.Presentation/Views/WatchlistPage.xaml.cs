using Microsoft.UI.Xaml.Controls;
using FinancialApp.Presentation.ViewModels;

namespace FinancialApp.Presentation.Views
{
    public sealed partial class WatchlistPage : Page
    {
        public WatchlistPage()
        {
            InitializeComponent();
            DataContext = new WatchlistViewModel(App.Services.MarketData, DispatcherQueue);
        }
    }
}
