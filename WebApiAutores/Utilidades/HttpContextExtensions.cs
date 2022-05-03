using Microsoft.EntityFrameworkCore;

namespace WebApiAutores.Utilidades
{
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametrosPaginacionEnCabecera<T>(this HttpContext context,
            IQueryable<T> queryable)
        {
            if(context == null) { throw new ArgumentNullException(nameof(context)); }

            double cantidad = await queryable.CountAsync();
            context.Response.Headers.Add("cantidadTotalRegistros", cantidad.ToString());
        }
    }
}
