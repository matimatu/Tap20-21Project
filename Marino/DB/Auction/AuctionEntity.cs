using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marino.DB
{
    public class AuctionEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SiteName { get; set; }
        [Required]
        public double CurrentPrice { get; set; }
        [Required]
        public string ShortDescription { get; set; }
        [Required]
        public double Cmo { get; set; }
        [Required]
        public DateTime EndsOn { get; set; }
        public virtual UserEntity Seller { get; set; }
        public int SellerId { get; set; }  
        public virtual UserEntity CurrentWinner { get; set; }
        public int? CurrentWinnerId { get; set; }  
        public virtual SiteEntity Site { get; set; }
    }   
}
