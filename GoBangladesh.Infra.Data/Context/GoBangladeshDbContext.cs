using GoBangladesh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Infra.Data.Context
{
   public class GoBangladeshDbContext : DbContext
    {
        public GoBangladeshDbContext(DbContextOptions<GoBangladeshDbContext> options)
          : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<MenuCrud> MenuCruds { get; set; }
        public DbSet<AccessControl> AccessControls { get; set; }
        public DbSet<MailHost> MailHosts { get; set; }   
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<PassengerCardMapping> PassengerCardMappings { get; set; }
        public DbSet<StaffBusMapping> StaffBusMappings { get; set; }
        public DbSet<OneTimePassword> OneTimePasswords { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
