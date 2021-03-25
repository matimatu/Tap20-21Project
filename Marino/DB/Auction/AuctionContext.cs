using System.Data.Entity;

namespace Marino.DB
{
    internal class AuctionContext : BaseContext
    {
        public AuctionContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<AuctionEntity> Auctions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AuctionConf());
            modelBuilder.Configurations.Add(new UserConf());
        }
    }
}
