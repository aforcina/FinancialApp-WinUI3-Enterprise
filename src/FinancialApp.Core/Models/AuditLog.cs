using System;

namespace FinancialApp.Core.Models
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid EntityId { get; private set; }
        public string EntityType { get; private set; }
        public string Action { get; private set; }
        public string UserId { get; private set; }
        public string ChangeDescription { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string IpAddress { get; private set; }
        public string SystemInfo { get; private set; }

        public AuditLog(
            Guid entityId,
            string entityType,
            string action,
            string userId,
            string changeDescription,
            string ipAddress = null,
            string systemInfo = null)
        {
            if (entityId == Guid.Empty) throw new ArgumentException("Entity ID is required.", nameof(entityId));
            if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("Entity type is required.", nameof(entityType));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.", nameof(action));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));

            Id = Guid.NewGuid();
            EntityId = entityId;
            EntityType = entityType;
            Action = action;
            UserId = userId;
            ChangeDescription = changeDescription ?? "N/A";
            Timestamp = DateTime.UtcNow;
            IpAddress = ipAddress;
            SystemInfo = systemInfo;
        }
    }
}
