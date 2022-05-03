using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]

    [Route("api/v1/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> usermanager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashServices hashServices;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> usermanager, 
            IConfiguration configuration, SignInManager<IdentityUser> signInManager, 
            IDataProtectionProvider dataProtectionProvider,
            HashServices hashServices)
        {
            this.usermanager = usermanager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashServices = hashServices;
            dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_quizas_secreto");
        }
        /*
        [HttpGet("{hash/textoplano}")]

        public ActionResult RealizarHash (string textoPlano)
        {
            var resultado = hashServices.Hash(textoPlano);

            return Ok(new {
            
                textoPlano = textoPlano,
                Hash = resultado
            });
        }

        [HttpGet("encriptar")]

        public ActionResult Encriptar()
        {
            var textoPlano = "Felipe galivan";
            var textoCifrado = dataProtector.Protect(textoPlano);
            var textoDesencriptado = dataProtector.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }

        [HttpGet("encriptarPorTiempo")]

        public ActionResult encriptarPorTiempo()
        {
            var protectorLimitado = dataProtector.ToTimeLimitedDataProtector();

            var textoPlano = "Felipe galivan";
            var textoCifrado = protectorLimitado.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(6000);
            var textoDesencriptado = protectorLimitado.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }*/

        [HttpPost("registrar", Name = "registrarUsuario")]

        public async Task<ActionResult<RespuestaAutenticacion>> Registrar (CredencialesUsuario credencialesUsuario)
        {
            var usuario = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };

            var resultado = await usermanager.CreateAsync(usuario, credencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost ("login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login (CredencialesUsuario credencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken (credencialesUsuario);
            }
            else
            {
                return BadRequest("Login incorrecto.");
            }
        }

        [HttpGet ("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email",credencialesUsuario.Email)

            };

            var usuario = await usermanager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsDB = await usermanager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddMinutes(30);

            var securityToken = new JwtSecurityToken(issuer: null,audience:null, claims: claims, 
                expires: expiracion,signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion,
            };
        }

        [HttpPost("HacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin (EditarAdminDTO editarAdminDTO)
        {
            var usuario = await usermanager.FindByEmailAsync(editarAdminDTO.Email);
            await usermanager.AddClaimAsync(usuario, new Claim("EsAdmin", "1"));

            return NoContent();
        }

        [HttpPost("RemoverAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await usermanager.FindByEmailAsync(editarAdminDTO.Email);
            await usermanager.RemoveClaimAsync(usuario, new Claim("EsAdmin", "1"));

            return NoContent();
        }

    }
}
