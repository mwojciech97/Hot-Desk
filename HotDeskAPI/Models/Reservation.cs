using HotDeskAPI.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace HotDeskAPI.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        [Required]
        public Desk Desk { get; set; }
        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }
        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
    }
}
