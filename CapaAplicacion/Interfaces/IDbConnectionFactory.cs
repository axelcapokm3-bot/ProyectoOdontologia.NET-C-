namespace CapaAplicacion.Interfaces;

using System.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
