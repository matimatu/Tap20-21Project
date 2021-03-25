using System;
using System.ComponentModel.DataAnnotations;

namespace Marino.DB
{
    public class SessionEntity
    {
        public string Id { get; set; }
        [Required]
        public DateTime ValidUntil { get; set; }
        public virtual UserEntity User { get; set; }
        public int UserId { get; set; }
    }
}
