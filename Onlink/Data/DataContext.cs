using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Onlink.Models;

namespace Onlink.Data
{
    public class DataContext : DbContext
    {
        public DataContext (DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Onlink.Models.Employee> Employee { get; set; } = default!;
        public DbSet<Onlink.Models.CheckInfo> CheckInfo { get; set; } = default!;
        public DbSet<Onlink.Models.Job> Job { get; set; } = default!;
        public DbSet<Onlink.Models.JobApplication> JobApplication { get; set; } = default!;
        public DbSet<Onlink.Models.Employer> Employer { get; set; } = default!;
        public DbSet<Onlink.Models.Post> Post { get; set; } = default!;
        public DbSet<Onlink.Models.Resume> Resume { get; set; } = default!;
        public DbSet<Onlink.Models.Certificate> Certificate { get; set; } = default!;
        public DbSet<Onlink.Models.EmployeeJob> EmployeeJob { get; set; } = default!;

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<LoginViewModel> LoginViewModel { get; set; } = default!;
        public DbSet<RegisterViewModel> RegisterViewModel { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Disable cascade delete globally first
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // Configure User-Employer relationship
            modelBuilder.Entity<Employer>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employer)
                .HasForeignKey<Employer>(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure User-Employee relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure other relationships as needed...

            modelBuilder.Entity<Post>()
            .HasMany(p => p.RelatedPosts)
            .WithOne(p => p.ParentPost)
            .HasForeignKey(p => p.ParentPostId)
            .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Resume>()
             .HasOne(r => r.Employee)
             .WithMany(e => e.Resumes)
             .HasForeignKey(r => r.EmployeeId)
             .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Resume>()
                .HasOne(r => r.Employer)
                .WithMany(e => e.Resume)
                .HasForeignKey(r => r.EmployerId)
                .OnDelete(DeleteBehavior.NoAction);



            base.OnModelCreating(modelBuilder);
        }
    }
}
