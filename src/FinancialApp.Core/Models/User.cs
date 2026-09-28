using System;
using System.Collections.Generic;

namespace FinancialApp.Core.Models
{
    public enum UserRole
    {
        Trader = 1,
        Manager = 2,
        Compliance = 3,
        Settlement = 4,
        Admin = 5
    }

    public class User
    {
        public string UserId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public List<UserRole> Roles { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User(string userId, string name, string email)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID is required.", nameof(userId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

            UserId = userId;
            Name = name;
            Email = email;
            Roles = new List<UserRole>();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void AssignRole(UserRole role)
        {
            if (!Roles.Contains(role))
                Roles.Add(role);
        }

        public void RemoveRole(UserRole role)
        {
            Roles.Remove(role);
        }

        public bool HasRole(UserRole role) => Roles.Contains(role);

        public void Deactivate() => IsActive = false;

        public void Reactivate() => IsActive = true;
    }
}
