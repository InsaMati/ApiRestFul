using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/autores")] // api/autores => ruta [controller] se sustituye por el nombre del controlador
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "obtenerAutoresv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable.OrderBy(autor => autor.Nombre).Paginar(paginacionDTO).ToListAsync();
            return  mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "obtenerAutorv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOconLibros>> Get(int id)
        {
            var autor = await context.Autores.Include(AutorDb => AutorDb.AutoresLibros)
                .ThenInclude(autorlibroDB => autorlibroDB.Libro)
                .FirstOrDefaultAsync(x => x.id == id);

            if (autor == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<AutorDTOconLibros>(autor);
           
            return dto;
        }


        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev1")]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre(string nombre)
        {
            var autor = await context.Autores.Where(autorBD => autorBD.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autor);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv1")]
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

        [HttpPost(Name = "crearAutorv1")]

        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO AutorCreacionDTO)
        {

            var existeAutor = await context.Autores.AnyAsync(x => x.Nombre == AutorCreacionDTO.Nombre);

            if (existeAutor)
            {
                return BadRequest($"Ya existe un autor con ese nombre {AutorCreacionDTO.Nombre}");
            }

            var autor = mapper.Map<Autor>(AutorCreacionDTO);

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutorv1", new { id = autor.id }, autorDTO);
        }

        [HttpDelete("{id:int}", Name = "borrarAutorv1")]

        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor { id = id });
            await context.SaveChangesAsync();

            return NoContent();

        }
    }
}
