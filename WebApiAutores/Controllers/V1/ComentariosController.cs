using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]

    [Route("api/v1/libros/{libroId:int}/comentarios")]


    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public ComentariosController(ApplicationDbContext dbContext, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            UserManager = userManager;
        }

        public UserManager<IdentityUser> UserManager { get; }

        [HttpGet (Name = "obtenerComentarioLibros")]

        public async Task<ActionResult<List<ComentarioDTO>>> get(int libroId,[FromQuery] PaginacionDTO paginacionDTO)
        {
            var existe = await dbContext.Libros.AnyAsync(LibroDb => LibroDb.id == libroId);

            if (!existe)
            {
                return NotFound();
            }

            var queryable = dbContext.Comentarios.Where(c => c.LibroId == libroId).AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var comentarios = await queryable.OrderBy(comentario => comentario.Id).Paginar(paginacionDTO).ToListAsync();

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

        [HttpPost (Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var usuario = await UserManager.FindByEmailAsync(email);

            var usuarioId = usuario.Id;

            var existe = await dbContext.Libros.AnyAsync(LibroDb => LibroDb.id == libroId);

            if (!existe)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);

            comentario.LibroId = libroId;
            comentario.usuarioId = usuarioId;

            dbContext.Add(comentario);

            await dbContext.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("obtenerComentario", new {id = comentario.Id, libroId = libroId},comentarioDTO);
        }

        [HttpPut ("{id:int}", Name = "actualizarComentario")]

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
