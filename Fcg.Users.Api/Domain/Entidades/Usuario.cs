using Domain.Enum;
using Domain.Shared;

namespace Domain.Entidades
{
    public sealed class Usuario
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Nome { get; private set; } = default!;
        public EmailStruct Email { get; private set; }
        public SenhaHashed SenhaHashed { get; private set; } = default!;
        public NivelAcessoEnum NivelAcesso { get; private set; } = NivelAcessoEnum.Usuario;
        public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

        private Usuario() { }

        public Usuario(string nome, EmailStruct email, SenhaHashed hash, NivelAcessoEnum level)
        {
            AlterarNome(nome);
            AlterarEmail(email);
            AlterarSenha(hash);
            AlterarNivel(level);
        }

        public void AlterarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome inválido.", nameof(nome));

            Nome = nome.Trim();
        }

        public void AlterarEmail(EmailStruct email)
        {
            Email = email;
        }

        public void AlterarNivel(NivelAcessoEnum nivel)
        {
            NivelAcesso = nivel;
        }

        public void AlterarSenha(SenhaHashed novaSenha)
        {
            SenhaHashed = novaSenha ?? throw new ArgumentNullException(nameof(novaSenha));
        }
    }
}
