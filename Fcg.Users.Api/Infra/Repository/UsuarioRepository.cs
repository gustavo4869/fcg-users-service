using Domain.Entidades;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using TechChallengeAPI.Infra;

namespace Fcg.Users.Api.Infra.Repository;
public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly UserDbContext _db;
    public UsuarioRepository(UserDbContext db) => _db = db;

    public async Task AddAsync(Usuario u, CancellationToken ct)
    {
        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var vo = EmailStruct.Create(email);
        return _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Email == vo, ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
    {
        var vo = EmailStruct.Create(email);
        return _db.Usuarios.AnyAsync(x => x.Email == vo, ct);
    }

    public async Task<List<Usuario>> ListAsync(CancellationToken ct)
        => await _db.Usuarios.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct);

    public async Task UpdateAsync(Usuario u, CancellationToken ct)
    {
        _db.Usuarios.Update(u);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Usuario u, CancellationToken ct)
    {
        _db.Usuarios.Remove(u);
        await _db.SaveChangesAsync(ct);
    }
}