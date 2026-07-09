var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();
postgres.AddDatabase("entries");
postgres.AddDatabase("media");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");
var redis = builder.AddRedis("redis");

builder.AddContainer("minio", "minio/minio")
    .WithEndpoint(9000, 9000, name: "api")
    .WithEndpoint(9001, 9001, name: "console")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin");

builder.AddProject<Projects.Triplog_Entries_Api>("triplog-entries-api")
    .WithReference(postgres).WaitFor(postgres)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(redis).WaitFor(redis);

builder.AddProject<Projects.Triplog_Media_Api>("triplog-media-api");

builder.Build().Run();
