using Camar.Domain.Common;
namespace Camar.Domain.Members;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string FullName { get; private set; }
    public string PasswordHash { get; private set; }
    public MembershipPlan MembershipPlan { get; private set; }
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Documento fiscal, imprescindible para poder facturarle.</summary>
    public TaxId TaxId { get; private set; }

    public PhoneNumber Phone { get; private set; }

    public PostalCode PostalCode { get; private set; }

    /// <summary>
    /// Cuenta para domiciliar. Es opcional a proposito: quien paga cada reserva al momento
    /// no tiene por que dar sus datos bancarios, y pedirlos sin necesitarlos es una barrera
    /// gratuita en el alta.
    /// </summary>
    public BankAccount? BankAccount { get; private set; }

    public User(
        string email,
        string fullName,
        string passwordHash,
        MembershipPlan membershipPlan,
        TaxId taxId,
        PhoneNumber phone,
        PostalCode postalCode,
        DateTimeOffset createdAt,
        BankAccount? bankAccount = null)
    {
        Id = Guid.CreateVersion7();
        Email = Guard.NotBlank(email).Trim().ToLowerInvariant();
        FullName = Guard.NotBlank(fullName).Trim();
        PasswordHash = Guard.NotBlank(passwordHash);
        MembershipPlan = membershipPlan;
        Role = UserRole.Member;
        TaxId = taxId;
        Phone = phone;
        PostalCode = postalCode;
        BankAccount = bankAccount;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Da permisos de administracion. No se puede hacer desde el registro publico:
    /// tiene que venir de una operacion deliberada.
    /// </summary>
    public void PromoteToAdmin()
    {
        if (Role == UserRole.Admin)
            throw new InvalidOperationException("El usuario ya es administrador.");

        Role = UserRole.Admin;
    }

    /// <summary>Añade o cambia la cuenta de domiciliacion.</summary>
    public void SetBankAccount(BankAccount? bankAccount) => BankAccount = bankAccount;
}
