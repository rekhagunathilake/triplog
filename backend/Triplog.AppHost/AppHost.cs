var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", port: 5432) // Aspire remembers the password across restarts
    .WithDataVolume()
    .WithPgAdmin();

postgres.AddDatabase("entries");
postgres.AddDatabase("media");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
var redis = builder.AddRedis("redis");

var minio = builder.AddContainer("minio", "minio/minio")
    .WithEnvironment("MINIO_SERVER_URL", "http://localhost:9000")
    .WithEnvironment("MINIO_API_CORS_ALLOW_ORIGIN", "http://localhost:3000")
    .WithEndpoint(9000, 9000, name: "api", scheme: "http")
    .WithEndpoint(9001, 9001, name: "console", scheme: "http")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithVolume("triplog-minio-data", "/data");

var minioApi = minio.GetEndpoint("api");

builder.AddProject<Projects.Triplog_Entries_Api>("triplog-entries-api")
    .WithReference(postgres).WaitFor(postgres)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(redis).WaitFor(redis);

builder.AddProject<Projects.Triplog_Media_Api>("triplog-media-api")
    .WithReference(postgres).WaitFor(postgres)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WaitFor(minio)
    .WithEnvironment("Minio__Endpoint", minioApi)
    .WithEnvironment("Minio__RootUser", "minioadmin")
    .WithEnvironment("Minio__RootPassword", "minioadmin");

builder.Build().Run();
