using CapaDominio.Entidades;

namespace CapaAplicacion.Seguridad.Interfaces;

public interface IJwtProvider
{
    (string Token, DateTime ExpirationUtc) GenerarToken(Usuario usuario);
    string GenerarRefreshToken();
}
