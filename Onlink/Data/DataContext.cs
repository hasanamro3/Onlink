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
     
    }
}
