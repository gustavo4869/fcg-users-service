using Application.Auth.Provider;
using Application.Usuarios.Validator;
using Fcg.Users.Api.Infra.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using TechChallengeAPI.Infra;
using TechChallengeAPI.Middleware;

namespace TechChallengeAPI.Setup
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiCore(this IServiceCollection services, IConfiguration cfg)
        {
            var connectionString = cfg.GetConnectionString("DefaultConnection") ?? "Data Source=fcg.db";

            services.AddDbContext<UserDbContext>(o => o.UseSqlite(connectionString));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddValidatorsFromAssembly(typeof(CriarUsuarioValidator).Assembly);

            services.AddTransient<ErrorMiddleware>();
            services.AddTransient<RequestLoggingMiddleware>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FIAP Cloud Games API",
                    Version = "v1",
                    Description = "MVP – Cadastro/Autenticação de Usuário e Cadastro de Jogo"
                });

                var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
                if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Envie: Bearer {seu_token}"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { 
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        }, 
                        Array.Empty<string>() 
                    }
                });
            });

            var key = cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key ausente.");
            if (Encoding.UTF8.GetBytes(key).Length < 32)
                throw new InvalidOperationException("Jwt:Key precisa ter >= 32 bytes.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            services.AddAuthorization(o =>
            {
                o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            });

            services.AddHealthChecks().AddDbContextCheck<UserDbContext>(name: "efcore-db", failureStatus: HealthStatus.Unhealthy);

            return services;
        }
    }
}
