using HBYWCZP2024.Data.Models;
using LinxUniverse.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HBYWCZP2024.Data.Models;

namespace HBYWCZP2024.Data
{
    public class LinxContext : LinxUserDBContext<LinxUser>
    {
        private const string connectString = @"Server=(localdb)\MSSQLLocalDB;Database=LXDB_HBYWCZP2024;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Connect Timeout=500";


        public LinxContext(DbContextOptions<LinxContext> options) : base(options)
        {

        }

        public LinxContext()
        {
                
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectString);
            base.OnConfiguring(optionsBuilder);

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PalletLog>().OwnsMany(
               poke => poke.BatteryInfo,
               onNav =>
               {
                   onNav.ToJson();

               });
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<WarningLog> WarningLogs { get; set; }

        public DbSet<WcsLog> WcsLogs { get; set; }
        public DbSet<OKNGCnt> OKNGCnts { get; set; }
        public DbSet<PalletLog> PalletLogs { get; set; }
        public DbSet<NgBeltCellLog> NgBeltCellLogs { get; set; }
    }
}