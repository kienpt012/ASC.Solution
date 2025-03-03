using ASC.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ASC.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MasterDataKey>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });

            builder.Entity<MasterDataValue>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<ServiceRequest>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<Product>()
                .HasKey(p => p.ProductId);
            builder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Oil Change" },
                new Product { ProductId = 2, Name = "Tire Rotation" },
                new Product { ProductId = 3, Name = "Brake Inspection" }
            );
            base.OnModelCreating(builder);
        }
    }

}
