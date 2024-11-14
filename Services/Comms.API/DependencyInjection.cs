namespace Comms.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCarter();

            services.AddExceptionHandler<CustomExceptionHandler>();
            services.AddHealthChecks()
                .AddMongoDb(configuration.GetConnectionString("Database")!);

            services.AddCors(options => options.AddPolicy("corsapp", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));


            return services;
        }

        public static WebApplication UseApiServices(this WebApplication app)
        {
            app.UseCors("corsapp");
            app.MapCarter();

            app.UseExceptionHandler(options => { });
            app.UseHealthChecks("/health",
                new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

            return app;
        }
    }
}
