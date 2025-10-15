using GoBangladesh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<PassengerCardMapping> PassengerCardMappings { get; set; }
        public DbSet<StaffBusMapping> StaffBusMappings { get; set; }
        public DbSet<OneTimePassword> OneTimePasswords { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<PassengerCardHistory> PassengerCardHistory { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<OrganizationCardBalance> OrganizationCardBalance { get; set; }
        public DbSet<OrganizationSettlement> OrganizationSettlement { get; set; }
        public DbSet<CardDue> CardDue { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoicePayment> InvoicePayment { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Stoppage> Stoppages { get; set; }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Route>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.RoutePath)
                    .HasColumnType("geography");
            });
        }
    }
}
