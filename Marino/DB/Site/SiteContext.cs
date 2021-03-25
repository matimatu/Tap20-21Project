using System.Data.Entity;

namespace Marino.DB
{
    internal class SiteContext : BaseContext
    {
        public SiteContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<SiteEntity> Sites { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SiteConf());
            modelBuilder.Configurations.Add(new UserConf());
            modelBuilder.Configurations.Add(new AuctionConf());
        }
    }
}
