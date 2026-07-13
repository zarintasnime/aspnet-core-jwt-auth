using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedLIbrary.Models;

namespace CoreAuthAndAuthUsingJWTToken.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Experience> Experiences { get; set; }
    }
}
