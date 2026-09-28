using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FinancialApp.Application.Services;
using FinancialApp.Core.Interfaces;
using FinancialApp.Core.Models;

namespace FinancialApp.Presentation.ViewModels
{
    public class TradeApprovalViewModel : INotifyPropertyChanged
    {
        private readonly IApprovalWorkflowService _approvalService;
        private readonly IAuditService _auditService;
        private readonly string _currentUserId;

        private ObservableCollection<TradeRequest> _pendingTrades = new ObservableCollection<TradeRequest>();

        public TradeApprovalViewModel(IApprovalWorkflowService approvalService, IAuditService auditService, string currentUserId)
        {
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _currentUserId = currentUserId ?? throw new ArgumentNullException(nameof(currentUserId));

            ApproveTradeCommand = new RelayCommand(async _ => await ApproveTrade(), CanApproveTrade);
            RejectTradeCommand = new RelayCommand(async _ => await RejectTrade(), CanRejectTrade);
            RefreshCommand = new RelayCommand(async _ => await RefreshPendingTrades(), _ => !IsBusy);

            LoadPendingTrades();
        }

        public ObservableCollection<TradeRequest> PendingTrades
        {
            get => _pendingTrades;
            set { _pendingTrades = value; OnPropertyChanged(); }
        }

        private TradeRequest _selectedTrade;
        public TradeRequest SelectedTrade
        {
            get => _selectedTrade;
            set
            {
                _selectedTrade = value;
                OnPropertyChanged();
                RejectionReason = string.Empty;
            }
        }

        private string _rejectionReason;
        public string RejectionReason
        {
            get => _rejectionReason;
            set { _rejectionReason = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand ApproveTradeCommand { get; }
        public ICommand RejectTradeCommand { get; }
        public ICommand RefreshCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CanApproveTrade(object obj) => SelectedTrade != null && SelectedTrade.Status == TradeStatus.PendingApproval && !IsBusy;
        private bool CanRejectTrade(object obj) => SelectedTrade != null && SelectedTrade.Status == TradeStatus.PendingApproval && !IsBusy;

        private void LoadPendingTrades()
        {
            // In a real app, this would fetch from a service
            // For demo, we'll add sample trades
            PendingTrades.Clear();
            StatusMessage = "Loaded pending trades for approval.";
        }

        private async Task ApproveTrade()
        {
            if (SelectedTrade == null) return;

            IsBusy = true;
            try
            {
                await _approvalService.ApproveTradeAsync(SelectedTrade, _currentUserId);
                await _auditService.LogActionAsync(
                    SelectedTrade.Id,
                    "TradeRequest",
                    "Approved",
                    _currentUserId,
                    $"Trade approved: {SelectedTrade.Symbol} {SelectedTrade.Quantity}@{SelectedTrade.LimitPrice}");

                PendingTrades.Remove(SelectedTrade);
                StatusMessage = $"Trade {SelectedTrade.Id:N} approved.";
                SelectedTrade = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error approving trade: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RejectTrade()
        {
            if (SelectedTrade == null || string.IsNullOrWhiteSpace(RejectionReason))
            {
                StatusMessage = "Please select a trade and provide a rejection reason.";
                return;
            }

            IsBusy = true;
            try
            {
                await _approvalService.RejectTradeAsync(SelectedTrade, _currentUserId, RejectionReason);
                await _auditService.LogActionAsync(
                    SelectedTrade.Id,
                    "TradeRequest",
                    "Rejected",
                    _currentUserId,
                    $"Trade rejected. Reason: {RejectionReason}");

                PendingTrades.Remove(SelectedTrade);
                StatusMessage = $"Trade {SelectedTrade.Id:N} rejected.";
                SelectedTrade = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error rejecting trade: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshPendingTrades()
        {
            IsBusy = true;
            try
            {
                LoadPendingTrades();
                StatusMessage = "Pending trades refreshed.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
