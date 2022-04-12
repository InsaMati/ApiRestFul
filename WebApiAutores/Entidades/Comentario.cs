namespace WebApiAutores.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }

        public string contenido { get; set; }

        public int LibroId { get; set; }

        // Propiedad de Navegacion

        public Libro libro { get; set; }
    }
}
