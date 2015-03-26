using System.Data.Entity;

namespace GongChaWebApplication.Models
{
    public class GongChaDbContext : DbContext
    {
        public GongChaDbContext()
            : base("name=DefaultConnection")
        {
        }

        public DbSet<Sales> SalesItems { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<CashOutflow> CashOutflowItems { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockHistory> StockHistories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<TransferOrder> TransferOrders { get; set; }
        public DbSet<TransferOrderDetail> TransferOrderDetails { get; set; }
        public DbSet<MemberProfile> MemberProfiles { get; set; }
        public DbSet<MemberRate> MemberRates { get; set; }
        public DbSet<WageType> WageTypes { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<SalesTarget> SalesTargets { get; set; }
        public DbSet<MemberUnavailability> MemberUnavailabilities { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}