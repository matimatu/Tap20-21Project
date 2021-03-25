using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.DB
{
    public class SiteEntity
    {
        [MinLength(DomainConstraints.MinSiteName), MaxLength(DomainConstraints.MaxSiteName)]
        [Key]
        public string Name { get; set; }
        [Required]
        public double MinBidIncr { get; set; }
        [Required, Range(DomainConstraints.MinTimeZone, DomainConstraints.MaxTimeZone)]
        public int Timezone { get; set; }
        [Required]
        public int SessionExp { get; set; }
        public virtual ICollection<UserEntity> Users { get; set; }
        public virtual ICollection<AuctionEntity> Auctions { get; set; }
    }
}
