namespace Domain.Shared;

public sealed class SenhaHashed
{
    public string Hash { get; private set; } = default!;

    private SenhaHashed() { }

    public SenhaHashed(string hash) => Hash = hash;

    public static SenhaHashed FromPlain(string plain)
        => new(BCrypt.Net.BCrypt.HashPassword(plain));

    public bool Verify(string plain) => BCrypt.Net.BCrypt.Verify(plain, Hash);
}
