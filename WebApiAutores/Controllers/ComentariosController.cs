using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]

    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public ComentariosController(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        

        [HttpGet]

        public async Task<ActionResult<List<ComentarioDTO>>> get(int libroId)
        {

            var existe = await dbContext.Libros.AnyAsync(LibroDb => LibroDb.id == libroId);

            if (!existe)
            {
                return NotFound();
            }

            var comentarios = await dbContext.Comentarios.Where(c => c.LibroId == libroId).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id:int}", Name = "obtenerComentario")]

        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            var comentario = await dbContext.Comentarios.FirstOrDefaultAsync(comentarioDB => comentarioDB.Id == id);

            if(comentario == null)
            {
                return NotFound();
            }

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]

        public async Task<IActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existe = await dbContext.Libros.AnyAsync(LibroDb => LibroDb.id == libroId);

            if (!existe)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);

            comentario.LibroId = libroId;

            dbContext.Add(comentario);

            await dbContext.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("obtenerComentario", new {id = comentario.Id, libroId = libroId},comentarioDTO);
        }

        [HttpPut ("{id:int}")]

        public async Task<IActionResult> Put(int libroId,int id ,ComentarioCreacionDTO ComentarioCreacionDTO)
        {
            var existe = await dbContext.Libros.AnyAsync(LibroDb => LibroDb.id == libroId);

            if (!existe)
            {
                return NotFound();
            }

            var existeComentario = await dbContext.Comentarios.AnyAsync(ComentarioDB => ComentarioDB.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(ComentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;
            dbContext.Update(comentario);
            dbContext.SaveChanges();

            return NoContent();
        }
    }
}
