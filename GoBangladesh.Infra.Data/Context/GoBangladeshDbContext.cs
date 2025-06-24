using GoBangladesh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DbSet<InviteUser> InviteUsers { get; set; }       
        public DbSet<Department> Departments { get; set; }       
        public DbSet<Designation> Designations { get; set; }       
        public DbSet<Employee> Employees { get; set; }       
        public DbSet<AssetType> AssetTypes { get; set; }       
        public DbSet<AssetStatus> AssetStatuses { get; set; }       
        public DbSet<MaintenanceType> MaintenanceTypes { get; set; }       
        public DbSet<MailHost> MailHosts { get; set; }   
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignVolunteerMapping> CampaignVolunteerMappings { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<FileModelMapping> FileModelMappings { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<News> News { get; set; }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
