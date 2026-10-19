using System.Reflection;
using Microsoft.Data.Sqlite;

namespace PrestamoBiblioteca.Persistencia;

internal sealed class InicializadorBaseDatos {
    public string RutaBaseDatos { get; } = Environment.GetEnvironmentVariable("PRESTAMO_BIBLIOTECA_DB")
        ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UMG", "PrestamosBiblioteca", "biblioteca.db");

    public string CadenaConexion => new SqliteConnectionStringBuilder {
        DataSource = RutaBaseDatos,
        ForeignKeys = true
    }.ToString();

    public void Inicializar() {
        var directorio = Path.GetDirectoryName(RutaBaseDatos);
        if (!string.IsNullOrEmpty(directorio)) Directory.CreateDirectory(directorio);

        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = LeerScriptTablas();
        comando.ExecuteNonQuery();
    }

    private static string LeerScriptTablas()
    {
        var ensamble = Assembly.GetExecutingAssembly();
        var recurso = ensamble.GetManifestResourceNames()
            .Single(nombre => nombre.EndsWith(
                "Crear_Tablas.sql",
                StringComparison.OrdinalIgnoreCase));

        using var stream = ensamble.GetManifestResourceStream(recurso)
            ?? throw new InvalidOperationException("ERROR");

        using var lector = new StreamReader(stream);
        return lector.ReadToEnd();
    }
}