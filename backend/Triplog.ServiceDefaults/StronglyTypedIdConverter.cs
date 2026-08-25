using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Triplog.ServiceDefaults;

/// <summary>
/// Serializes any readonly record struct T(Guid Value) as a plain Guid string,
/// so wire shape becomes { "id": "guid" } instead of { "id": { "value": "guid" } }.
/// </summary>
public sealed class StronglyTypedIdConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsValueType) return false;
        var valueProp = typeToConvert.GetProperty("Value");
        var ctor = typeToConvert.GetConstructor([typeof(Guid)]);
        return valueProp?.PropertyType == typeof(Guid) && ctor is not null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(StronglyTypedIdConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class StronglyTypedIdConverter<TId> : JsonConverter<TId>
    where TId : struct
{
    // Compiled once per TId, cached in the static field - reflection cost paid once
    private static readonly Func<Guid, TId> Wrap = BuildWrap();
    private static readonly Func<TId, Guid> Unwrap = BuildUnwrap();

    public override TId Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions __)
        => Wrap(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions _)
        => writer.WriteStringValue(Unwrap(value));

    private static Func<Guid, TId> BuildWrap()
    {
        // Equivalent to: g => new TId(g)
        var ctor = typeof(TId).GetConstructor([typeof(Guid)])!;
        var param = Expression.Parameter(typeof(Guid), "g");
        return Expression.Lambda<Func<Guid, TId>>(Expression.New(ctor, param), param).Compile();
    }

    private static Func<TId, Guid> BuildUnwrap()
    {
        // Equivalent to: id => id.Value
        var prop = typeof(TId).GetProperty("Value")!;
        var param = Expression.Parameter(typeof(TId), "id");
        return Expression.Lambda<Func<TId, Guid>>(Expression.Property(param, prop), param).Compile();
    }
}