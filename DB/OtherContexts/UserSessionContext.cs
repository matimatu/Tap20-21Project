using System.Data.Entity;

namespace Marino.DB
{
    internal class UserSessionContext : BaseContext
    {
        public UserSessionContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<SessionEntity> Sessions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SessionConf());
            modelBuilder.Configurations.Add(new UserConf());
            //modelBuilder.Configurations.Add(new SiteConf());
        }
    }
}
