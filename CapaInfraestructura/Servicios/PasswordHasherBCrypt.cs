using BCrypt.Net;
using CapaAplicacion.Interfaces;

namespace CapaInfraestructura.Servicios;

public class PasswordHasherBCrypt : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
