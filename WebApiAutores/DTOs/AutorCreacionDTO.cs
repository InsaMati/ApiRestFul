using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class AutorCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(maximumLength: 20, ErrorMessage = "El campo {0} no debe tener mas de {1} caracteres.")]
        public string Nombre { get; set; }
    }
}
