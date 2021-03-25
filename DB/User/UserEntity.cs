using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.DB
{
    public class UserEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  
        [MinLength(DomainConstraints.MinUserName), MaxLength(DomainConstraints.MaxUserName)]
        public string Name { get; set; }
        public string SiteName { get; set; }
        [Required, MinLength(Auxiliary.HashedPwOccupation), MaxLength(Auxiliary.HashedPwOccupation)]
        public string Password { get; set; }
        public virtual SiteEntity Site { get; set; }
        public virtual ICollection<SessionEntity> Sessions { get; set; }
        public virtual ICollection<AuctionEntity> AuctionsWon { get; set; }  
        public virtual ICollection<AuctionEntity> AuctionsCreated { get; set; }

    }
}
