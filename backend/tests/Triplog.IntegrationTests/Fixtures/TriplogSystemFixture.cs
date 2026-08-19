using Npgsql;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Triplog.IntegrationTests.Fixtures;

public class TriplogSystemFixture : IAsyncLifetime
{
    private const string PostgresPassword = "triplog-test";
    private const string MinioUser = "minioadmin";
    private const string MinioSecret = "minioadmin";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithPassword(PostgresPassword)
        .Build();

    private readonly RabbitMqContainer _rabbitmq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3")
        .Build();

    private readonly MinioContainer _minio = new MinioBuilder()
        .WithImage("minio/minio")
        .WithUsername(MinioUser)
        .WithPassword(MinioSecret)
        .Build();

    // Exposed connection details
    public string EntriesConnectionString { get; private set; } = null!;
    public string MediaConnectionString { get; private set; } = null!;
    public string RabbitMqConnectionString => _rabbitmq.GetConnectionString();
    public string MinioEndpoint => $"http://{_minio.Hostname}:{_minio.GetMappedPublicPort(9000)}";
    public string MinioAccessKey => MinioUser;
    public string MinioSecretKey => MinioSecret;


    // Exposed factories
    public EntriesApiFactory Entries { get; private set; } = null!;
    public MediaApiFactory Media { get; private set; } = null!;

    // Lifecycle

    public async Task InitializeAsync()
    {
        // Start all three containers in parallel — biggest single test speedup
        await Task.WhenAll(
            _postgres.StartAsync(),
            _rabbitmq.StartAsync(),
            _minio.StartAsync());

        // Testcontainers only creates the default 'postgres' database.
        // We need two named databases matching your production setup.
        await CreateDatabaseAsync("entries");
        await CreateDatabaseAsync("media");

        // Build per-database connection strings for the API factories to use
        EntriesConnectionString = WithDatabase(_postgres.GetConnectionString(), "entries");
        MediaConnectionString = WithDatabase(_postgres.GetConnectionString(), "media");

        Entries = new EntriesApiFactory(this);
        Media = new MediaApiFactory(this);
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbitmq.DisposeAsync().AsTask(),
            _minio.DisposeAsync().AsTask());
    }

    // Helpers
    private async Task CreateDatabaseAsync(string name)
    {
        // Connect to the default postgres database to issue CREATE DATABASE
        // (you can't CREATE DATABASE while connected to the target DB)
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE \"{name}\";";
        await cmd.ExecuteNonQueryAsync();
    }

    private static string WithDatabase(string baseConnectionString, string database)
    {
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = database,
        };
        return builder.ConnectionString;
    }
}
