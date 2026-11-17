namespace CapaInfraestructura.Persistencia;

using System.Data;
using System.Globalization;
using System.Reflection;
using CapaDominio.Entidades;
using Dapper;

public static class SqliteTypeHandlers
{
    private static readonly object _lock = new();
    private static bool _registrados;

    public static void AsegurarRegistrados()
    {
        if (_registrados) return;

        lock (_lock)
        {
            if (_registrados) return;

            SqlMapper.AddTypeHandler(new DateTimeTextoHandler());
            SqlMapper.AddTypeHandler(new DecimalInvarianteHandler());
            SqlMapper.AddTypeHandler(new TimeSpanTicksHandler());

            SqlMapper.SetTypeMap(
                typeof(Turno),
                new ConstructorForzadoTypeMap(
                    typeof(Turno),
                    new[]
                    {
                        typeof(int),
                        typeof(DateTime),
                        typeof(DateTime),
                        typeof(int),
                        typeof(int),
                        typeof(int),
                        typeof(EstadoTurno)
                    }));

            SqlMapper.SetTypeMap(
                typeof(Tratamiento),
                new ConstructorForzadoTypeMap(
                    typeof(Tratamiento),
                    new[]
                    {
                        typeof(int),
                        typeof(string),
                        typeof(decimal),
                        typeof(TimeSpan)
                    }));

            SqlMapper.SetTypeMap(
                typeof(TratamientoInsumo),
                new ConstructorForzadoTypeMap(
                    typeof(TratamientoInsumo),
                    new[]
                    {
                        typeof(int),
                        typeof(int),
                        typeof(int)
                    }));

            _registrados = true;
        }
    }

    private sealed class DateTimeTextoHandler : SqlMapper.TypeHandler<DateTime>
    {
        private const string Formato = "yyyy-MM-dd HH:mm:ss";

        public override DateTime Parse(object value)
        {
            string texto = value as string ?? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

            if (DateTime.TryParseExact(texto, Formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                return fecha;

            return DateTime.Parse(texto, CultureInfo.InvariantCulture);
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString(Formato, CultureInfo.InvariantCulture);
        }
    }

    private sealed class DecimalInvarianteHandler : SqlMapper.TypeHandler<decimal>
    {
        public override decimal Parse(object value)
        {
            if (value is string texto)
                return decimal.Parse(texto, NumberStyles.Number, CultureInfo.InvariantCulture);

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public override void SetValue(IDbDataParameter parameter, decimal value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString(CultureInfo.InvariantCulture);
        }
    }

    private sealed class TimeSpanTicksHandler : SqlMapper.TypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
        {
            return TimeSpan.FromTicks(Convert.ToInt64(value, CultureInfo.InvariantCulture));
        }

        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
        {
            parameter.DbType = DbType.Int64;
            parameter.Value = value.Ticks;
        }
    }

    private sealed class ConstructorForzadoTypeMap : SqlMapper.ITypeMap
    {
        private readonly DefaultTypeMap _mapaPorDefecto;
        private readonly ConstructorInfo _constructor;

        public ConstructorForzadoTypeMap(Type type, Type[] tiposParametros)
        {
            _mapaPorDefecto = new DefaultTypeMap(type);

            _constructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: tiposParametros,
                modifiers: null)
                ?? throw new InvalidOperationException(
                    $"No se encontró el constructor de {type.Name} con los tipos especificados.");
        }

        public ConstructorInfo? FindConstructor(string[] names, Type[] types) => _constructor;

        public ConstructorInfo? FindExplicitConstructor() => null;

        public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName) =>
            _mapaPorDefecto.GetConstructorParameter(constructor, columnName);

        public SqlMapper.IMemberMap? GetMember(string columnName) =>
            _mapaPorDefecto.GetMember(columnName);
    }
}
