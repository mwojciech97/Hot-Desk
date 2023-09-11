using HotDeskAPI.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;

namespace HotDeskAPI.Models
{
    public class Desk
    {
        public int Id { get; set; }
        [Required]
        public int DeskId { get; set; }
        [Required]
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public bool IsAvailable { get; set; }
    }
}
