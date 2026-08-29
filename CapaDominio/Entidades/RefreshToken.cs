namespace CapaDominio.Entidades;

public class RefreshToken
{
    public string Token { get; private set; }
    public DateTime ExpiracionUtc { get; private set; }
    public bool Revocado { get; private set; }
    public bool Usado { get; private set; }
    public int UsuarioId { get; private set; }

    public RefreshToken(string token, DateTime expiracionUtc, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("El token es obligatorio.");
        if (usuarioId <= 0) throw new ArgumentException("El usuario es inválido.");

        Token = token;
        ExpiracionUtc = expiracionUtc;
        UsuarioId = usuarioId;
    }

    public bool EstaActivo => !Revocado && !Usado && ExpiracionUtc > DateTime.UtcNow;

    public void Revocar() => Revocado = true;

    public void MarcarComoUsado() => Usado = true;
}
