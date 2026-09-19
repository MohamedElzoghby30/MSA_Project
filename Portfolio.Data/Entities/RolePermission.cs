using Portfolio.Data.Identity;

namespace Portfolio.Data.Entities
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }


        // Navigation Properties

        public ApplicationRole Role { get; set; } = null!;

        public Permission Permission { get; set; } = null!;
    }
}