using Camar.Domain.Common;
namespace Camar.Domain.Members
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string FullName { get; private set; }
        public string PasswordHash { get; private set; }
        public MembershipPlan MembershipPlan { get; private set; }
        public UserRole Role { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public User(string email, string fullName, string passwordHash, MembershipPlan membershipPlan, DateTimeOffset createdAt)
        {
            Id = Guid.CreateVersion7();
            Email = Guard.NotBlank(email).Trim().ToLowerInvariant();
            FullName = Guard.NotBlank(fullName).Trim();
            PasswordHash = Guard.NotBlank(passwordHash);
            MembershipPlan = membershipPlan;
            Role = UserRole.Member;
            CreatedAt = createdAt;
        }
    }
}
