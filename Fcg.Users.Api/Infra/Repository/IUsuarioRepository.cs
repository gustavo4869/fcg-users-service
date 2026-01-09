using Domain.Entidades;

namespace Fcg.Users.Api.Infra.Repository
{
    public interface IUsuarioRepository
    {
        Task AddAsync(Usuario u, CancellationToken ct);
        Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
        Task<List<Usuario>> ListAsync(CancellationToken ct);
        Task UpdateAsync(Usuario u, CancellationToken ct);
        Task DeleteAsync(Usuario u, CancellationToken ct);
    }
}
