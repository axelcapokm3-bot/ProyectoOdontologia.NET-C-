using CapaAplicacion.Interfaces;
using CapaAplicacion.Seguridad.Interfaces;
using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaAplicacion.Seguridad.Servicios;

public class AuthService : IAuthService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPaciente _repositorioPaciente;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPaciente repositorioPaciente,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioPaciente = repositorioPaciente;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserRequestDto request)
    {
        if (await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(request.NombreUsuario) is not null)
            throw new InvalidOperationException("El nombre de usuario ya existe.");

        if (await _repositorioUsuario.ObtenerPorEmailAsync(request.Email) is not null)
            throw new InvalidOperationException("El email ya existe.");

        var usuario = new Usuario(
            _repositorioUsuario.IdAutoIncrementoSincrono(),
            request.NombreUsuario,
            request.Email,
            _passwordHasher.Hash(request.Password),
            rol: RolUsuario.Paciente);

        await _repositorioUsuario.AgregarAsync(usuario);
        return await GenerarRespuestaAuthAsync(usuario);
    }

    public async Task<AuthResponseDto> RegistrarPacienteAsync(RegistroPacienteRequestDto request)
    {
        if (await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(request.NombreUsuario) is not null)
            throw new DomainException("Ya existe un usuario con ese nombre de usuario.");

        if (await _repositorioUsuario.ObtenerPorEmailAsync(request.Email) is not null)
            throw new DomainException("Ya existe un usuario con ese correo electrónico.");

        await _unitOfWork.IniciarTransaccionAsync();

        try
        {
            int idUsuario = _repositorioUsuario.IdAutoIncrementoSincrono();

            Usuario usuario = Usuario.CrearParaRegistroPublico(
                idUsuario,
                request.NombreUsuario,
                request.Email,
                _passwordHasher.Hash(request.Password));

            await _repositorioUsuario.AgregarAsync(usuario);

            int idPaciente = 0;

            Paciente paciente = new Paciente(
                idPaciente,
                request.Nombre,
                request.Apellido,
                request.FechaNacimiento,
                request.Telefono,
                request.Email,
                usuarioId: usuario.Id);

            await _repositorioPaciente.AgregarPaciente(paciente);

            await _unitOfWork.CommitAsync();

            return await GenerarRespuestaAuthAsync(usuario);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

   public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
{
    var usuario = await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(request.NombreUsuario);

    if (usuario == null)
    {
        usuario = await _repositorioUsuario.ObtenerPorEmailAsync(request.NombreUsuario);
    }

    if (usuario == null || !_passwordHasher.Verify(request.Password, usuario.PasswordHash))
    {
        throw new UnauthorizedAccessException("Credenciales inválidas.");
    }

    if (!usuario.EstaActivo)
    {
        throw new UnauthorizedAccessException("La cuenta de usuario se encuentra desactivada.");
    }

    return await GenerarRespuestaAuthAsync(usuario);
}
    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await _repositorioUsuario.ObtenerRefreshTokenAsync(request.RefreshToken)
            ?? throw new UnauthorizedAccessException("Refresh token invalido.");

        if (!refreshToken.EstaActivo)
            throw new UnauthorizedAccessException("Refresh token expirado o revocado.");

        var usuario = await _repositorioUsuario.ObtenerPorIdAsync(refreshToken.UsuarioId)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

        if (!usuario.EstaActivo)
            throw new UnauthorizedAccessException("La cuenta de usuario se encuentra desactivada.");

        refreshToken.MarcarComoUsado();
        await _repositorioUsuario.RevocarRefreshTokenAsync(request.RefreshToken);

        return await GenerarRespuestaAuthAsync(usuario);
    }

    private async Task<AuthResponseDto> GenerarRespuestaAuthAsync(Usuario usuario)
    {
        var (accessToken, accessExpiration) = _jwtProvider.GenerarToken(usuario);
        var refreshToken = _jwtProvider.GenerarRefreshToken();
        var refreshExpiration = DateTime.UtcNow.AddDays(7);

        await _repositorioUsuario.GuardarRefreshTokenAsync(
            usuario.Id,
            new RefreshToken(refreshToken, refreshExpiration, usuario.Id));

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpirationUtc = accessExpiration,
            RefreshTokenExpirationUtc = refreshExpiration,
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString()
        };
    }
}
