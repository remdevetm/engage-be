using UserAuthService.Data;
using UserAuthService.Data.Interfaces;
using UserAuthService.Repositories;
using UserAuthService.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using UserAuthService.Services;
using UserAuthService.Services.Interfaces;
using UserAuthService.Models.Model;

namespace UserAuthService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register OtpSettings
            services.Configure<OtpSettings>(Configuration.GetSection("OtpSettings"));

            // Register MongoDB client
            services.AddSingleton<IMongoClient>(sp =>
                new MongoClient(Configuration.GetSection("MongoDb:ConnectionString").Value));

            // Register MongoDB context
            services.AddSingleton<IMongoDBContext, MongoDbContext>();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILoginActivityRepository, LoginActivityRepository>();

            services.AddScoped<IHashingService, HashingService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddControllers();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserAuthService API", Version = "v1" });

                // Define the BearerAuth scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            Console.WriteLine("################################################################");
            Console.WriteLine($"Auth Server URL: {AuthServerConfig.URL}");
            Console.WriteLine($"HTTPS Required: {AuthServerConfig.HTTPS_REQUIRED}");
            Console.WriteLine("################################################################");

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            // Configure JWT Bearer Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = AuthServerConfig.URL;
                    options.Audience = AuthServerConfig.API_NAME;
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RoleClaimType = "client_role"  // This maps the client_role claim to roles
                    };
                });

            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            // Optional: Middleware to log roles for debugging
            //app.Use(async (context, next) =>
            //{
            //    var user = context.User;
            //    if (user.Identity.IsAuthenticated)
            //    {
            //        var roles = user.FindAll("client_role").Select(c => c.Value);
            //        Console.WriteLine($"User roles: {string.Join(", ", roles)}");
            //    }
            //    await next();
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
