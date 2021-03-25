using System.Data.Entity;

namespace Marino.DB
{
    internal class SiteUserContext : BaseContext
    {
        public SiteUserContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<SiteEntity> Sites { get; set; }
        public DbSet<UserEntity> Users{ get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SiteConf());
            modelBuilder.Configurations.Add(new UserConf());
        }
    }
}
