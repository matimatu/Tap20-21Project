using System.Data.Entity;

namespace Marino.DB
{
    internal class AuctionUserContext : BaseContext
    {
        public AuctionUserContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<AuctionEntity> Auctions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AuctionConf());
            modelBuilder.Configurations.Add(new UserConf());
            modelBuilder.Configurations.Add(new SessionConf());
        }
    }
    
}
