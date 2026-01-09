using Application.Usuarios.Request;
using Application.Usuarios.Response;
using Domain.Entidades;
using Domain.Enum;
using Domain.Shared;
using Fcg.Users.Api.Infra.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using static TechChallengeAPI.Contratos.Responses.CommonResponses;

namespace TechChallengeAPI.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static IEndpointRouteBuilder MapUsuariosEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/usuarios").WithTags("Usuários");

            g.MapPost("",
                async Task<Results<
                    Created<UsuarioCriadoResponse>,
                    ValidationProblem,
                    BadRequest<ErrorResponse>>> (
                    CriarUsuarioRequest req,
                    IUsuarioRepository users,
                    CancellationToken ct) =>
                {
                    if (await users.ExistsByEmailAsync(req.Email, ct))
                        return TypedResults.BadRequest(new ErrorResponse("E-mail já cadastrado"));

                    var nivel = (req.NivelAcesso?.ToLowerInvariant()) switch
                    {
                        "admin" => NivelAcessoEnum.Administrador,
                        _ => NivelAcessoEnum.Usuario
                    };

                    var u = new Usuario(
                        req.Nome,
                        EmailStruct.Create(req.Email),
                        SenhaHashed.FromPlain(req.Senha),
                        nivel);

                    await users.AddAsync(u, ct);

                    return TypedResults.Created($"/api/v1/usuarios/{u.Id}",
                        new UsuarioCriadoResponse(u.Id, u.Nome, u.Email.Address, u.NivelAcesso.ToString()));
                })
            .WithValidation<CriarUsuarioRequest>()
            .Accepts<CriarUsuarioRequest>("application/json")
            .Produces<UsuarioCriadoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

            var admin = g.MapGroup("").RequireAuthorization("AdminOnly");

            admin.MapGet("/{id:guid}",
                async Task<Results<Ok<UsuarioResponse>, NotFound>> (
                    Guid id, IUsuarioRepository users, CancellationToken ct) =>
                {
                    var u = await users.GetByIdAsync(id, ct);
                    if (u is null) return TypedResults.NotFound();

                    return TypedResults.Ok(new UsuarioResponse(
                        u.Id, u.Nome, u.Email.Address, u.NivelAcesso.ToString(), u.DataCriacao));
                })
            .Produces<UsuarioResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

            admin.MapGet("",
                async Task<Ok<IEnumerable<UsuarioResponse>>> (
                    IUsuarioRepository users, CancellationToken ct) =>
                {
                    var list = await users.ListAsync(ct);
                    var dto = list.Select(u => new UsuarioResponse(
                        u.Id, u.Nome, u.Email.Address, u.NivelAcesso.ToString(), u.DataCriacao));
                    return TypedResults.Ok(dto);
                })
            .Produces<IEnumerable<UsuarioResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

            admin.MapPut("/{id:guid}",
                async Task<Results<
                    Ok<UsuarioResponse>,
                    ValidationProblem,
                    BadRequest<ErrorResponse>,
                    NotFound>> (
                    AtualizarUsuarioRequest req,
                    Guid id,
                    IUsuarioRepository users,
                    CancellationToken ct) =>
                {
                    var u = await users.GetByIdAsync(id, ct);
                    if (u is null) return TypedResults.NotFound();

                    if (!string.IsNullOrWhiteSpace(req.Email))
                    {
                        var outro = await users.GetByEmailAsync(req.Email, ct);
                        if (outro is not null && outro.Id != id)
                            return TypedResults.BadRequest(new ErrorResponse("E-mail já cadastrado"));

                        u.AlterarEmail(EmailStruct.Create(req.Email));
                    }

                    if (!string.IsNullOrWhiteSpace(req.Nome))
                        u.AlterarNome(req.Nome);

                    if (!string.IsNullOrWhiteSpace(req.NivelAcesso))
                    {
                        var nivel = req.NivelAcesso.Equals("admin", StringComparison.OrdinalIgnoreCase)
                            ? NivelAcessoEnum.Administrador
                            : NivelAcessoEnum.Usuario;
                        u.AlterarNivel(nivel);
                    }

                    await users.UpdateAsync(u, ct);

                    return TypedResults.Ok(new UsuarioResponse(
                        u.Id, u.Nome, u.Email.Address, u.NivelAcesso.ToString(), u.DataCriacao));
                })
            .WithValidation<AtualizarUsuarioRequest>()
            .Accepts<AtualizarUsuarioRequest>("application/json")
            .Produces<UsuarioResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

            admin.MapDelete("/{id:guid}",
                async Task<Results<BadRequest<ErrorResponse>, NoContent, NotFound>> (
                    Guid id, IUsuarioRepository users, HttpContext httpContext, CancellationToken ct) =>
                {
                    var u = await users.GetByIdAsync(id, ct);
                    if (u is null) return TypedResults.NotFound();

                    var userIdClaim = httpContext.User.FindFirst("sub")?.Value ?? httpContext.User.FindFirst("id")?.Value;
                    if (Guid.TryParse(userIdClaim, out var authenticatedUserId))
                    {
                        if (authenticatedUserId == id)
                            return TypedResults.BadRequest(new ErrorResponse("Você não pode excluir a si mesmo."));
                    }

                    await users.DeleteAsync(u, ct);
                    return TypedResults.NoContent();
                })
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

            return app;
        }
    }
}