using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancialApp.Core.Models
{
    public enum TradeStatus
    {
        Draft = 0,
        Submitted = 1,
        PendingApproval = 2,
        Approved = 3,
        Rejected = 4,
        Executed = 5,
        Cancelled = 6,
        Failed = 7
    }

    public enum ApprovalLevel
    {
        Auto = 0,
        Manager = 1,
        Director = 2,
        Executive = 3
    }

    public class TradeRequest
    {
        public Guid Id { get; private set; }
        public Guid AccountId { get; private set; }
        public string Symbol { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal LimitPrice { get; private set; }
        public decimal MarketPrice { get; private set; }
        public string Currency { get; private set; }
        public TradeDirection Direction { get; private set; }
        public TradeStatus Status { get; private set; }
        public ApprovalLevel RequiredApprovalLevel { get; private set; }
        public string CreatedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string ApprovedByUserId { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string RejectionReason { get; private set; }
        public DateTime? ExecutedAt { get; private set; }
        public decimal? ExecutionPrice { get; private set; }
        public string Comments { get; private set; }

        public TradeRequest(
            Guid accountId,
            string symbol,
            decimal quantity,
            decimal limitPrice,
            decimal marketPrice,
            string currency,
            TradeDirection direction,
            string createdByUserId,
            ApprovalLevel requiredApprovalLevel = ApprovalLevel.Auto)
        {
            if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol is required.", nameof(symbol));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
            if (limitPrice < 0) throw new ArgumentOutOfRangeException(nameof(limitPrice), "Limit price cannot be negative.");
            if (marketPrice < 0) throw new ArgumentOutOfRangeException(nameof(marketPrice), "Market price cannot be negative.");
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
            if (string.IsNullOrWhiteSpace(createdByUserId)) throw new ArgumentException("User ID is required.", nameof(createdByUserId));

            Id = Guid.NewGuid();
            AccountId = accountId;
            Symbol = symbol.Trim().ToUpperInvariant();
            Quantity = quantity;
            LimitPrice = limitPrice;
            MarketPrice = marketPrice;
            Currency = currency.Trim().ToUpperInvariant();
            Direction = direction;
            Status = TradeStatus.Draft;
            CreatedByUserId = createdByUserId;
            CreatedAt = DateTime.UtcNow;
            RequiredApprovalLevel = requiredApprovalLevel;
        }

        public Money NotionalValue => new Money(Quantity * MarketPrice, Currency);

        public void Submit()
        {
            if (Status != TradeStatus.Draft)
                throw new InvalidOperationException("Only draft trades can be submitted.");
            Status = TradeStatus.Submitted;
        }

        public void RequestApproval(ApprovalLevel level)
        {
            if (Status != TradeStatus.Submitted)
                throw new InvalidOperationException("Only submitted trades can request approval.");
            RequiredApprovalLevel = level;
            Status = TradeStatus.PendingApproval;
        }

        public void Approve(string userId)
        {
            if (Status != TradeStatus.PendingApproval && Status != TradeStatus.Submitted)
                throw new InvalidOperationException("Trade is not in a state that can be approved.");
            Status = TradeStatus.Approved;
            ApprovedByUserId = userId;
            ApprovedAt = DateTime.UtcNow;
        }

        public void Reject(string userId, string reason)
        {
            if (Status != TradeStatus.PendingApproval && Status != TradeStatus.Submitted)
                throw new InvalidOperationException("Trade is not in a state that can be rejected.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.", nameof(reason));
            Status = TradeStatus.Rejected;
            ApprovedByUserId = userId;
            ApprovedAt = DateTime.UtcNow;
            RejectionReason = reason;
        }

        public void Execute(decimal executionPrice)
        {
            if (Status != TradeStatus.Approved)
                throw new InvalidOperationException("Only approved trades can be executed.");
            if (executionPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(executionPrice), "Execution price cannot be negative.");

            Status = TradeStatus.Executed;
            ExecutionPrice = executionPrice;
            ExecutedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string reason)
        {
            Status = TradeStatus.Failed;
            RejectionReason = reason;
        }

        public void Cancel()
        {
            if (Status == TradeStatus.Executed || Status == TradeStatus.Failed || Status == TradeStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel a trade in its current state.");
            Status = TradeStatus.Cancelled;
        }

        public void AddComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment cannot be empty.", nameof(comment));
            Comments = string.IsNullOrEmpty(Comments) ? comment : $"{Comments}\n{comment}";
        }
    }
}
