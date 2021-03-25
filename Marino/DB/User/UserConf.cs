using System.Data.Entity.ModelConfiguration;

namespace Marino.DB
{
    internal class UserConf: EntityTypeConfiguration<UserEntity>
    {
        public UserConf()
        {
            ToTable("Users");
            HasRequired(u => u.Site)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SiteName);
                //.WillCascadeOnDelete(false);
            HasIndex(u => new {u.Name, u.SiteName})
                .IsUnique(true);
        }
    }
}
