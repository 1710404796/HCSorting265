using HBYWCZP2024.Data.Models;
using LinxUniverse.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Data
{
    public class LinxContext : LinxUserDBContext<LinxUser>
    {
        private const string connectString = @"Server=(localdb)\MSSQLLocalDB;Database=LXDB_HBYWCZP2024;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Connect Timeout=500";


        public LinxContext(DbContextOptions<LinxContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectString);

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        public DbSet<WarningLog> WarningLogs { get; set; }

        public DbSet<WcsLog> WcsLogs { get; set; }

    }
}