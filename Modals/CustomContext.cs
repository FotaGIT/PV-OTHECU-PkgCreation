using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PkgCreation.Modals
{
    public partial class CustomContext : DbContext
    {
        public CustomContext()
        {
        }
        public CustomContext(DbContextOptions<CustomContext> options)
           : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // if (!optionsBuilder.IsConfigured)
            // {
            //     #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            //     optionsBuilder.UseMySql("server=portalapp-qa.cfl2ukbrlc9f.ap-south-1.rds.amazonaws.com;port=3306;database=PV_OTHECUFOTA;uid=portalappqa;password=Prt@!aPPq@@123;pooling=false;convert zero datetime=True;default command timeout=500;", x => x.EnableRetryOnFailure());
            // }

            IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .Build();

            if (!optionsBuilder.IsConfigured)
            {
                if (config.GetConnectionString("DefaultConnection") == null)
                {
                    throw new Exception("Connection string is empty!");
                }
                optionsBuilder.UseMySql(config.GetConnectionString("DefaultConnection"), x => x.ServerVersion("8.0.19-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FotaTblPackageDataDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<Appsettings>().HasNoKey().ToView(null);
            modelBuilder.Entity<ManualPkgres>().HasNoKey().ToView(null);
            modelBuilder.Entity<SingleIdDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<EmailDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<QueryResult>().HasNoKey().ToView(null);
        }
    }
}
