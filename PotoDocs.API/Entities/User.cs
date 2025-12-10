namespace PotoDocs.API.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public required int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}