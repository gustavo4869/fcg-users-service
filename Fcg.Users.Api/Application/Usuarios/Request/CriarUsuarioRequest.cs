using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usuarios.Request
{
    public sealed record CriarUsuarioRequest(string Nome, string Email, string Senha, string? NivelAcesso);
}
