using Application.Auth.Provider;
using Application.Auth.Request;
using Application.Auth.Response;
using Fcg.Users.Api.Infra.Repository;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TechChallengeAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/auth").WithTags("Autenticação");

            g.MapPost("/login",
                async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> (
                    LoginRequest req,
                    IUsuarioRepository users,
                    IJwtProvider jwt,
                    CancellationToken ct) =>
                {
                    var u = await users.GetByEmailAsync(req.Email.ToLowerInvariant(), ct);
                    if (u is null || !u.SenhaHashed.Verify(req.Senha))
                        return TypedResults.Unauthorized();

                    var (token, exp) = jwt.Create(u);
                    return TypedResults.Ok(new AuthResponse(token, exp, u.NivelAcesso.ToString()));
                })
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithSummary("Autentica usuário")
            .WithDescription("Valida credenciais e retorna um JWT (claims: sub, email, role, exp).");

            return app;
        }
    }
}
