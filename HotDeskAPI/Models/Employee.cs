using HotDeskAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace HotDeskAPI.Model
{
    public class Employee
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surrname { get; set; }
    }
}
