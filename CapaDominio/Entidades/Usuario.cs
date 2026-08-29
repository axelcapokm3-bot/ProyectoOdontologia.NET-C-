namespace CapaDominio.Entidades;

public enum RolUsuario
{
    Paciente = 1,
    Odontologo = 2,
    Recepcionista = 3,
    Admin = 4
}

public class Usuario
{
    public int Id { get; private set; }
    public string NombreUsuario { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public RolUsuario Rol { get; private set; }
    public bool EstaActivo { get; private set; }
    public List<RefreshToken> RefreshTokens { get; private set; } = new();

    public Usuario(int id, string nombreUsuario, string email, string passwordHash, RolUsuario rol, bool estaActivo = true)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser positivo.");
        if (string.IsNullOrWhiteSpace(nombreUsuario)) throw new ArgumentException("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("El hash de contraseña es obligatorio.");
        if (!Enum.IsDefined(typeof(RolUsuario), rol)) throw new ArgumentException("El rol especificado no es válido.");

        Id = id;
        NombreUsuario = nombreUsuario;
        Email = email;
        PasswordHash = passwordHash;
        Rol = rol;
        EstaActivo = estaActivo;
    }

    public void CambiarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("El hash de contraseña es obligatorio.");
        PasswordHash = passwordHash;
    }

    public void AgregarRefreshToken(RefreshToken refreshToken)
    {
        RefreshTokens.Add(refreshToken);
    }
}
