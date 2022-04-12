using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]

    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "ObtenerLibro")]

        public async Task<ActionResult<LibroDTOconAutores>> Get(int id)
        {
            // es como hacer un inner join

            var libro = await context.Libros
                .Include(LibroDb => LibroDb.AutoresLibros)
                .ThenInclude(AutorLibroDb => AutorLibroDb.Autor).
                FirstOrDefaultAsync(x => x.id == id);

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroDTOconAutores>(libro);
        }

        [HttpPost]

        public async Task<ActionResult<Libro>> Post(LibroCreacionDTO libroCreacionDTO)
        {

            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            var autoresId = await context.Autores.Where(autorBD => libroCreacionDTO.AutoresIds.Contains(autorBD.id)).
                Select(x => x.id).ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresId.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("ObtenerLibro", new { id = libro.id }, libroDTO);
        }

        [HttpPut("{id:int}")]

        public async Task<IActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.id == id);

            if (libroDB == null)
            {
                return NotFound();
            }

            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }
    }
}
