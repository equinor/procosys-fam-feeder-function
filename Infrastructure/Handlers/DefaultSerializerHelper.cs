using System.Text.Json;

namespace Infrastructure.Handlers;

public static class DefaultSerializerHelper
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = { new DateOnlyJsonConverter() }
    };
}