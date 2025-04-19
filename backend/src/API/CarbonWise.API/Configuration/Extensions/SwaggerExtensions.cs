using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarbonWise.API.Configuration.Extensions
{
    public static class SwaggerExtensions
    {
        public static void ConfigureSwaggerOptions(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CarbonWise API",
                Version = "v1",
                Description = "CarbonWise API i�in REST Servisleri",
                Contact = new OpenApiContact
                {
                    Name = "CarbonWise",
                    Email = "info@carbonwise.com"
                }
            });
        }
    }
}