using Microsoft.EntityFrameworkCore;
using VaultAPI.Models;

namespace VaultAPI
{
    public class GuardianDbContext : DbContext
    {
        public GuardianDbContext(DbContextOptions<GuardianDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Secret> Secrets { get; set; }
        public DbSet<SecretAccess> SecretAccesses { get; set; }
        public DbSet<SecretAuditLog> SecretAuditLogs { get; set; }
    }
}
