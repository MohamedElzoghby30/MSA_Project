using Microsoft.AspNetCore.Identity;

namespace Portfolio.Data.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }
}