using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using travelingExperience.Entity;

namespace travelingExperience.Models
{
    public class UserSavedTravel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Travel")]
        public int TravelId { get; set; }
        public Travel Travel { get; set; }

    }
}
