using Microsoft.EntityFrameworkCore;
using DNN.Models;
using DNN.Controllers;

namespace DNN.Data
{
    public class DNNDbContext : DbContext
    {
        public DNNDbContext(DbContextOptions<DNNDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: configure table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");

            // Seed data for roles (safe in EF Core)
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Student" }
            );
        }
    }
}