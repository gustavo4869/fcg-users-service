using Application.Usuarios.Request;
using FluentValidation;

namespace Application.Usuarios.Validator
{
    public sealed class CriarUsuarioValidator : AbstractValidator<CriarUsuarioRequest>
    {
        public CriarUsuarioValidator()
        {
            RuleFor(x => x.Nome).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Senha)
                .MinimumLength(8)
                .Matches("[A-Z,a-z]").WithMessage("Deve conter letras")
                .Matches("[0-9]").WithMessage("Deve conter números")
                .Matches("[^a-zA-Z0-9]").WithMessage("Deve conter caractere especial");
            RuleFor(x => x.NivelAcesso).Must(x => x is null || x is "admin" or "usuario")
                .WithMessage("NivelAcesso deve ser 'admin' ou 'usuario'.");
        }
    }
}
