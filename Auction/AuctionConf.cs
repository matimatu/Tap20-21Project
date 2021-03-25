using System.Data.Entity.ModelConfiguration;

namespace Marino.DB
{
    internal class AuctionConf : EntityTypeConfiguration<AuctionEntity>
    {
        public AuctionConf()
        {
            ToTable("Auctions");
            HasRequired(a => a.Site)
                .WithMany(s => s.Auctions)
                .HasForeignKey(a => a.SiteName)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Seller)
                .WithMany(u => u.AuctionsCreated)
                .HasForeignKey(a => a.SellerId);
                //.WillCascadeOnDelete(false);

                HasOptional(a => a.CurrentWinner)
                    .WithMany(u => u.AuctionsWon)
                    .HasForeignKey(a => a.CurrentWinnerId)
                .WillCascadeOnDelete(false);

                Property(a => a.Cmo)
                    .HasDatabaseGeneratedOption(0);

        }
    }
}
