using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        private const string ConnectionString = "DefaultConnection";

        public static IServiceCollection ApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options => 
            {
                options.UseSqlite(configuration.GetConnectionString(ConnectionString));
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }        
    }
}