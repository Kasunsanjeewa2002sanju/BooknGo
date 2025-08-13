using System.ComponentModel.DataAnnotations;

namespace BooknGo.Models
{
    public class Booking
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string? Name { get; set; }          
        public string? Description { get; set; }   
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }

        public int? Capacity { get; set; }     
        public string? Category { get; set; }

    }
}
