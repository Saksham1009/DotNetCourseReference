using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Data
{
    public class DataContextEF : DbContext
    {
        private readonly IConfiguration _configuration;

        public DataContextEF(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual DbSet<User> Users {get; set;}
        public virtual DbSet<UserJobInfo> UserJobInfos {get; set;}
        public virtual DbSet<UserSalary> UserSalaries {get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (!dbContextOptionsBuilder.IsConfigured)
            {
                dbContextOptionsBuilder
                    .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"), 
                    dbContextOptionsBuilder => dbContextOptionsBuilder.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TutorialAppSchema");

            modelBuilder.Entity<User>()
                .ToTable("Users", "TutorialAppSchema")
                .HasKey(user => user.UserId);

            modelBuilder.Entity<UserJobInfo>()
                .HasKey(user => user.UserId);

            modelBuilder.Entity<UserSalary>()
                .HasKey(user => user.UserId);
        }
    }
}