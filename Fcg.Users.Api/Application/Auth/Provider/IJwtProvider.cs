using Domain.Entidades;

namespace Application.Auth.Provider
{
    public interface IJwtProvider
    {
        (string token, DateTime expires) Create(Usuario usuario, TimeSpan? ttl = null);
    }
}
