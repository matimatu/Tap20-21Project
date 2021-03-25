using System.Data.Entity;

namespace Marino.DB
{
    internal class SessionContext : BaseContext
    {
        public SessionContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<SessionEntity> Sessions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SessionConf());
            modelBuilder.Configurations.Add(new UserConf());
        }
    }
}
