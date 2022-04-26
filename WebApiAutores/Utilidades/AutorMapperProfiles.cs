using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Utilidades
{
    public class AutorMapperProfiles : Profile
    {
        public AutorMapperProfiles()
        {
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorDTO>();
            CreateMap<Autor, AutorDTOconLibros>().ForMember(autorDTO => autorDTO.Libro, opciones =>
            opciones.MapFrom(MapAutorDTOLibros));

            CreateMap<LibroCreacionDTO, Libro>().ForMember(libro => libro.AutoresLibros, opciones =>
            opciones.MapFrom(MapAutoresLibros));

            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOconAutores>().ForMember(LibroDTO => LibroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();

            CreateMap<ComentarioCreacionDTO,Comentario>();
            CreateMap<Comentario, ComentarioDTO>();

        }

        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            if (autor.AutoresLibros == null) return resultado;

            foreach (var autorLibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO()
                {
                    id = autorLibro.LibroId,
                    titulo = autorLibro.Libro.titulo
                });
            }

            return resultado;
        }

        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO LibroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutorLibro>();

            if (LibroCreacionDTO.AutoresIds == null) return resultado;

            foreach (var autorId in LibroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro()
                {
                    AutorId = autorId
                });
            }
 
            return resultado;
        }

        private List<AutorDTO> MapLibroDTOAutores(Libro Libro, LibroDTO LibroDto)
        {
            var resultado = new List<AutorDTO>();

            if (Libro.AutoresLibros == null) return resultado;

            foreach (var autorlibro in Libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = autorlibro.AutorId,
                    Nombre = autorlibro.Autor.Nombre
                });
            }

            return resultado;
        }
    }
}
