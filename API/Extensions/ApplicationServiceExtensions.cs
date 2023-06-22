using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        private const string ConnectionString = "DefaultConnection";
        private const string CloudinarySettings = "CloudinarySettings";

        public static IServiceCollection ApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options => 
            {
                options.UseSqlite(configuration.GetConnectionString(ConnectionString));
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPhotoService, PhotoService>();
            
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings));
            
            return services;
        }        
    }
}