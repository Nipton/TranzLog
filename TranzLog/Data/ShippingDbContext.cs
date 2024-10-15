using Microsoft.EntityFrameworkCore;
using TranzLog.Models;

namespace TranzLog.Data
{
    public class ShippingDbContext : DbContext
    {
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Consignee> Consignees { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Cargo> Cargo { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Models.Route> Routes { get; set; }
        public DbSet<TransportOrder> TransportOrders { get; set; }
        public DbSet<User> Users { get; set; }


        public ShippingDbContext(DbContextOptions<ShippingDbContext> dbContextOptions) : base(dbContextOptions) { }
    }
}
