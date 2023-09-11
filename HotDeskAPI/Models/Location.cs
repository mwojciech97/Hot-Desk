using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotDeskAPI.Models
{
    public class Location
    {
        public int Id { get; set; }
        [Index(IsUnique = true)]
        [Required]
        public string LocationName { get; set; }
    }
}
