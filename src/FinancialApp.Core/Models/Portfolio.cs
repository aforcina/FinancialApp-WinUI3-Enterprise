using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinancialApp.Core.Models
{
    public class Portfolio : INotifyPropertyChanged
    {
        private ObservableCollection<Position> _positions;
        private decimal _totalValue;
        private decimal _totalCostBasis;
        private decimal _dailyPnL;
        private decimal _unrealizedPnL;
        private decimal _buyingPower;

        public ObservableCollection<Position> Positions
        {
            get => _positions;
            set { _positions = value; OnPropertyChanged(); }
        }

        public decimal TotalValue
        {
            get => _totalValue;
            set { _totalValue = value; OnPropertyChanged(); }
        }

        public decimal TotalCostBasis
        {
            get => _totalCostBasis;
            set { _totalCostBasis = value; OnPropertyChanged(); }
        }

        public decimal DailyPnL
        {
            get => _dailyPnL;
            set { _dailyPnL = value; OnPropertyChanged(); }
        }

        public decimal UnrealizedPnL
        {
            get => _unrealizedPnL;
            set { _unrealizedPnL = value; OnPropertyChanged(); }
        }

        public decimal BuyingPower
        {
            get => _buyingPower;
            set { _buyingPower = value; OnPropertyChanged(); }
        }

        public Portfolio()
        {
            Positions = new ObservableCollection<Position>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
