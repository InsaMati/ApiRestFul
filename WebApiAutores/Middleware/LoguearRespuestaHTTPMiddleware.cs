namespace WebApiAutores.Middleware
{
    public static class LoguearRespuestaMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
        }
    }
    public class LoguearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LoguearRespuestaHTTPMiddleware> logger;

        public LoguearRespuestaHTTPMiddleware(RequestDelegate siguiente,
            ILogger<LoguearRespuestaHTTPMiddleware> Logger)
        {
            this.siguiente = siguiente;
            logger = Logger;
        }

        // Invoke o Invoke Async

        public async Task Invoke(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cuerpoOriginal = contexto.Response.Body;
                contexto.Response.Body = ms;

                await siguiente(contexto);

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();

                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginal);
                contexto.Response.Body = cuerpoOriginal;

                logger.LogInformation(respuesta);

            }
        }
    }
}
