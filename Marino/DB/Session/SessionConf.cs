using System.Data.Entity.ModelConfiguration;

namespace Marino.DB
{
    public class SessionConf : EntityTypeConfiguration<SessionEntity>
    {
        public SessionConf()
        {
            ToTable("Sessions");
            HasRequired(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId);
            //.WillCascadeOnDelete(false);
        }
    }
}
