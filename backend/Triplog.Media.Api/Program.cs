using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Triplog.Media.Api.Auth;
using Triplog.Media.Api.Endpoints;
using Triplog.Media.Api.Http;
using Triplog.Media.Application;
using Triplog.Media.Application.Abstractions;
using Triplog.Media.Infrastructure;
using Triplog.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, ConfiguredOwnerCurrentUser>();

builder.Services.AddTriplogAuth(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new StronglyTypedIdConverterFactory());
});

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<TriplogMediaDbContext>();
    await db.Database.MigrateAsync();

    var minio = scope.ServiceProvider.GetRequiredService<IMinioClient>();
    const string bucket = "triplog-media";
    var exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
    if (!exists)
        await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapMediaEndpoints();

app.Run();