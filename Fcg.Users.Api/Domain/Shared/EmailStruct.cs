using System.Text.RegularExpressions;

namespace Domain.Shared
{
    public readonly record struct EmailStruct(string Address)
    {
        public static EmailStruct Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("E-mail não pode ser vazio.");

            var normalized = email.Trim().ToLowerInvariant();

            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$");
            if (!regex.IsMatch(normalized))
                throw new ArgumentException("Formato de e-mail inválido.");

            return new EmailStruct(normalized);
        }
    }
}
