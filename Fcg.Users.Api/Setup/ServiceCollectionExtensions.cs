using Application.Auth.Provider;
using Application.Usuarios.Validator;
using Fcg.Users.Api.Infra.Repository;
using Fcg.Users.Api.Setup;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
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
            // Application Insights - Configuração completa
            // Tentar múltiplos formatos de configuração (compatibilidade com diferentes ambientes)
            var connectionString = cfg["ApplicationInsights:ConnectionString"]
                ?? cfg["ApplicationInsights__ConnectionString"] 
                ?? cfg["APPLICATIONINSIGHTS_CONNECTION_STRING"]
                ?? Environment.GetEnvironmentVariable("ApplicationInsights__ConnectionString")
                ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

            // Log de debug para diagnóstico (será enviado ao Application Insights quando configurado)
            Console.WriteLine($"[DEBUG] Application Insights Connection String found: {!string.IsNullOrWhiteSpace(connectionString)}");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine($"[DEBUG] Connection String length: {connectionString.Length} characters");
                Console.WriteLine($"[DEBUG] Contains InstrumentationKey: {connectionString.Contains("InstrumentationKey")}");
            }
            else
            {
                Console.WriteLine("[WARNING] Application Insights Connection String NOT FOUND - Telemetry disabled");
                Console.WriteLine("[DEBUG] Checked keys: ApplicationInsights:ConnectionString, ApplicationInsights__ConnectionString, APPLICATIONINSIGHTS_CONNECTION_STRING");
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.ConnectionString = connectionString;
                    
                    // Controle de sampling
                    options.EnableAdaptiveSampling = cfg.GetValue<bool>("ApplicationInsights:EnableAdaptiveSampling", true);
                    
                    // Coleta de performance counters (CPU, memória, etc)
                    options.EnablePerformanceCounterCollectionModule = cfg.GetValue<bool>("ApplicationInsights:EnablePerformanceCounterCollectionModule", true);
                    
                    // Rastreamento de dependências (HTTP, SQL, etc)
                    options.EnableDependencyTrackingTelemetryModule = cfg.GetValue<bool>("ApplicationInsights:EnableDependencyTrackingTelemetryModule", true);
                    
                    // Coleta de heartbeat para monitoramento de health
                    options.EnableHeartbeat = true;
                    
                    // Coleta automática de requisições HTTP
                    options.EnableRequestTrackingTelemetryModule = true;
                    
                    // Coleta de eventos de exceção
                    options.EnableEventCounterCollectionModule = true;
                });
                
                // Configurar TelemetryInitializer para nome da aplicação
                services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameTelemetryInitializer("fcg-users"));
                
                // Integrar ILogger com Application Insights
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) => 
                            config.ConnectionString = connectionString,
                        configureApplicationInsightsLoggerOptions: (options) => { }
                    );
                });
                
                Console.WriteLine("[SUCCESS] Application Insights configured successfully!");
            }
            
            var connectionStringDb = cfg.GetConnectionString("DefaultConnection") ?? "Data Source=fcg.db";

            services.AddDbContext<UserDbContext>(o => o.UseSqlite(connectionStringDb));

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
