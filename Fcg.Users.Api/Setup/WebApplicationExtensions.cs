using Domain.Entidades;
using Domain.Enum;
using Domain.Shared;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Text.Json;
using TechChallengeAPI.Endpoints;
using TechChallengeAPI.Infra;
using TechChallengeAPI.Middleware;

namespace TechChallengeAPI.Setup
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseApiCore(this WebApplication app)
        {
            app.UseMiddleware<ErrorMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    var prefix = httpReq.Headers["X-Forwarded-Prefix"].FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        swagger.Servers = new List<OpenApiServer>
                        {
                            new() { Url = prefix }
                        };
                    }
                    else
                    {
                        // Acesso direto (sem gateway): mantém raiz
                        swagger.Servers = new List<OpenApiServer>
                        {
                            new() { Url = "/" }
                        };
                    }
                });
            }); ;
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("v1/swagger.json", "FIAP Cloud Games v1");
                opt.DisplayRequestDuration();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            ApplyMigrationsAndSeed(app);

            app.MapHealthChecks("/health");

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = async (ctx, report) =>
                {
                    ctx.Response.ContentType = "application/json";
                    var payload = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(kvp => new
                        {
                            name = kvp.Key,
                            status = kvp.Value.Status.ToString(),
                            error = kvp.Value.Exception?.Message
                        })
                    };
                    await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
                }
            });

            return app;
        }

        public static WebApplication MapV1Endpoints(this WebApplication app)
        {
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            var api = app.MapGroup("/api/v1");
            api.MapUsuariosEndpoints();
            api.MapAuthEndpoints();
            return app;
        }

        private static void ApplyMigrationsAndSeed(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            db.Database.Migrate();

            if (app.Environment.IsDevelopment())
            {
                var adminEmail = EmailStruct.Create("admin@fcg.com");
                var existeAdmin = db.Usuarios.Any(u => u.Email == adminEmail);
                if (!existeAdmin)
                {
                    var admin = new Usuario(
                        nome: "Admin",
                        email: adminEmail,
                        hash: SenhaHashed.FromPlain("Admin@123"),
                        level: NivelAcessoEnum.Administrador);
                    db.Usuarios.Add(admin);
                    db.SaveChanges();
                }
            }
        }
    }
}
