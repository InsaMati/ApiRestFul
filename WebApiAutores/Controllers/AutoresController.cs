using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")] // api/autores => ruta [controller] se sustituye por el nombre del controlador
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<List<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name ="obtenerAutor")]
        public async Task<ActionResult<AutorDTOconLibros>> Get (int id)
        {
            var autor = await context.Autores.Include(AutorDb => AutorDb.AutoresLibros)
                .ThenInclude(autorlibroDB => autorlibroDB.Libro)
                .FirstOrDefaultAsync(x => x.id == id);

            if(autor == null)
            {
                return NotFound();
            }

            return mapper.Map<AutorDTOconLibros>(autor);
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDTO>>> Get(string nombre)
        {
            var autor = await context.Autores.Where(autorBD => autorBD.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autor);
        }

        [HttpPut("{id:int}")]

        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.id = id;

            context.Update(autor);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost] 

        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO AutorCreacionDTO)
        {

            var existeAutor = await context.Autores.AnyAsync(x => x.Nombre == AutorCreacionDTO.Nombre);

            if(existeAutor)
            {
                return BadRequest($"Ya existe un autor con ese nombre {AutorCreacionDTO.Nombre}");
            }

            var autor = mapper.Map<Autor>(AutorCreacionDTO);

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutor", new { id = autor.id }, autorDTO);
        }

        [HttpDelete("{id:int}")]

        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor { id = id });
            await context.SaveChangesAsync();

            return Ok();

        }
    }
}
