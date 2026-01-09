namespace Application.Auth.Response
{
    public sealed record AuthResponse(string Token, DateTime ExpiraEm, string NivelAcesso);
}
