using Domain.Entidades;
using Fcg.Users.Api.Infra.Configs;
using Fcg.Users.Api.Infra.Events;
using Microsoft.EntityFrameworkCore;

namespace TechChallengeAPI.Infra
{
    public sealed class UserDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<EventEntity> Events => Set<EventEntity>();

        public UserDbContext(DbContextOptions<UserDbContext> opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioConfig());
            modelBuilder.ApplyConfiguration(new EventEntityConfig());
        }
    }
}
