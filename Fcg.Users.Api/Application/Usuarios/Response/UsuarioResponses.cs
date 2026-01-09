namespace Application.Usuarios.Response
{
    public sealed record UsuarioCriadoResponse(Guid Id, string Nome, string Email, string NivelAcesso);
    public sealed record UsuarioResponse(Guid Id, string Nome, string Email, string NivelAcesso, DateTime DataCriacao);
}
