using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Sample.Models
{
    public class CreateBookViewModel
    {
        public string Name { get; set; }

        public string Author { get; set; }
    }

    public class CreateBookDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Author { get; set; }
    }
}