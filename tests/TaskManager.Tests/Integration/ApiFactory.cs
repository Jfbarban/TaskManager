using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string Usuario = "usuario-test";
    public const string Password = "Password-De-Test-123";

    // La conexión se mantiene abierta: así la base en memoria vive mientras dure la fábrica.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configuración propia de tests: no dependemos de user-secrets ni de la máquina.
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TaskManager.Tests",
                ["Jwt:Audience"] = "TaskManager.Tests.Api",
                ["Jwt:Key"] = "clave-secreta-solo-para-tests-0123456789-abcdef",
                ["Jwt:ExpirationMinutes"] = "30",
                ["Auth:Usuario"] = Usuario,
                ["Auth:Password"] = Password
            }));

        builder.ConfigureServices(services =>
        {
            QuitarConfiguracionDeBaseDeDatos(services);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    /// <summary>Crea el esquema y deja las tablas vacías (sin los datos de ejemplo).</summary>
    public void InicializarBaseDeDatos()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.EnsureCreated();
        db.Tareas.ExecuteDelete();
        db.Responsables.ExecuteDelete();
    }

    // EF Core 9+ registra, además de DbContextOptions<T>, un objeto de configuración
    // (IDbContextOptionsConfiguration<T>) con el UseSqlServer original. Hay que quitar
    // ambos; si no, EF vería dos proveedores (SQL Server y SQLite) y fallaría.
    private static void QuitarConfiguracionDeBaseDeDatos(IServiceCollection services)
    {
        var aQuitar = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                (d.ServiceType.IsGenericType
                 && d.ServiceType.GenericTypeArguments.Contains(typeof(AppDbContext))
                 && d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")))
            .ToList();

        foreach (var descriptor in aQuitar)
            services.Remove(descriptor);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}