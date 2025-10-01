
namespace UserPermissions.Domain.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<UserRole> UserRoles { get; set; } = new();
}
