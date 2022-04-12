using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class LibroDTO
    {
        public int id { get; set; }
        [Required]
        [StringLength(maximumLength: 250)]
        public string titulo { get; set; }

        //public List<ComentarioDTO> comentarios { get; set; }
    }
}
