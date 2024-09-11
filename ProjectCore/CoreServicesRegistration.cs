
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCore.Data;
using ProjectCore.Shared.RequestContext;
namespace ProjectCore
{
    public static class CoreServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("WebApp") ?? throw new InvalidOperationException("Connection string 'WebApp' not found.")));
            //services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            //{
            //    var requestContext = serviceProvider.GetRequiredService<IRequestContext>();
            //    var tenant = requestContext.Zone;

            //    // Dynamische selectie van de connectiestring op basis van tenant
            //    var connectionString = configuration.GetConnectionString(tenant)
            //        ?? throw new InvalidOperationException($"Connection string for tenant '{tenant}' not found.");

            //    options.UseSqlServer(connectionString);
            //});
            services.AddMapster();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
