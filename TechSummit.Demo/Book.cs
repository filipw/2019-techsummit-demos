using System.ComponentModel.DataAnnotations;

namespace TechSummit.Demo
{
    public class Book
    {
        [Required]
        public string Lang { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }
    }
}
