using System.Data.Entity.ModelConfiguration;

namespace Marino.DB
{
    public class SiteConf : EntityTypeConfiguration<SiteEntity>
    {
        public SiteConf()
        {
            ToTable("Sites");
        }
        
    }
}
