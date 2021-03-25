using System.Data.Entity;

namespace Marino.DB.Total 
{
    internal class TotalContext : BaseContext
    {
        public TotalContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<SiteEntity> Sites { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<AuctionEntity> Auctions { get; set; }
        public DbSet<SessionEntity> Sessions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SiteConf());
            modelBuilder.Configurations.Add(new UserConf());
            modelBuilder.Configurations.Add(new SessionConf());
            modelBuilder.Configurations.Add(new AuctionConf());
        }
    }
}
