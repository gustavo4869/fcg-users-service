using Application.Usuarios.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usuarios.Validator
{
    public sealed class AtualizarUsuarioValidator : AbstractValidator<AtualizarUsuarioRequest>
    {
        public AtualizarUsuarioValidator()
        {
            RuleFor(x => x.Nome)
                .MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Nome));

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.NivelAcesso)
                .Must(v => string.Equals(v, "admin", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(v, "usuario", StringComparison.OrdinalIgnoreCase))
                .When(x => !string.IsNullOrWhiteSpace(x.NivelAcesso))
                .WithMessage("NivelAcesso deve ser 'admin' ou 'usuario'.");
        }
    }
}
