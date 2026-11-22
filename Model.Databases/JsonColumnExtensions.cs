using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Databases;

/// <summary>
/// Properties kept as JSON text in a column of their own: what a shared type keeps for other systems, and what a layout
/// was made from.
/// </summary>
internal static class JsonColumnExtensions
{
    // Layout.JsonSerializerOptions is an extension property, which an expression tree can't hold.
    private static readonly JsonSerializerOptions ContractOptions = Layout.JsonSerializerOptions;

    extension(PropertyBuilder<Dictionary<string, JsonElement>?> property)
    {
        /// <summary>
        /// Unknown properties or references to other registers: kept as they were read, as a JSON object, or null when
        /// there are none.
        /// </summary>
        public PropertyBuilder<Dictionary<string, JsonElement>?> HasJsonProperties() =>
            property.HasConversion(
                v => v == null || v.Count == 0 ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(v, (JsonSerializerOptions?)null),
                new ValueComparer<Dictionary<string, JsonElement>?>(
                    (a, b) => Text(a) == Text(b),
                    v => Text(v).GetHashCode(StringComparison.Ordinal),
                    v => v == null ? null : new Dictionary<string, JsonElement>(v)));
    }

    extension<T>(PropertyBuilder<T> property)
    {
        /// <summary>
        /// A part of the layout written as the shared model writes it on its own (<c>Layout.JsonSerializerOptions</c>):
        /// what refers to a location does so by its id, which <c>layout.RestoreReferences()</c> resolves after reading.
        /// </summary>
        public PropertyBuilder<T> HasContractJson() =>
            property.HasConversion(
                v => JsonSerializer.Serialize(v, ContractOptions),
                v => JsonSerializer.Deserialize<T>(v, ContractOptions)!,
                new ValueComparer<T>(
                    (a, b) => ContractText(a) == ContractText(b),
                    v => ContractText(v).GetHashCode(StringComparison.Ordinal),
                    v => JsonSerializer.Deserialize<T>(ContractText(v), ContractOptions)!));
    }

    private static string Text(Dictionary<string, JsonElement>? value) =>
        value is null || value.Count == 0 ? string.Empty : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null);

    private static string ContractText<T>(T value) => JsonSerializer.Serialize(value, ContractOptions);
}
