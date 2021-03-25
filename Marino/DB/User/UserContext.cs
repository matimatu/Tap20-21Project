using System.Data.Entity;

namespace Marino.DB
{
    internal class UserContext : BaseContext
    {
        public UserContext(string connectionString) : base(connectionString)
        {
        }
        public DbSet<UserEntity> Users { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new UserConf());
            modelBuilder.Configurations.Add(new SessionConf());
        }


    }
}
