namespace Application.Auth.Response
{
    public sealed record LoginResponse(string Token, DateTime ExpiraEm, string NivelAcesso);
}
