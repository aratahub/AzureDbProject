using Microsoft.EntityFrameworkCore;
using Entities;


namespace Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        
        }

        public DbSet<DbOrder> DbOrder { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Set default precision for all decimal properties
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
