﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Extensions.AI;

/// <summary>Source-generated JSON type information.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
[JsonSerializable(typeof(AzureAIChatToolJson))]
[JsonSerializable(typeof(IDictionary<string, object?>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(float[]))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(sbyte[]))]
internal sealed partial class JsonContext : JsonSerializerContext
{
    /// <summary>Gets the <see cref="JsonSerializerOptions"/> singleton used as the default in JSON serialization operations.</summary>
    private static readonly JsonSerializerOptions _defaultToolJsonOptions = CreateDefaultToolJsonOptions();

    /// <summary>Gets JSON type information for the specified type.</summary>
    /// <remarks>
    /// This first tries to get the type information from <paramref name="firstOptions"/>,
    /// falling back to <see cref="_defaultToolJsonOptions"/> if it can't.
    /// </remarks>
    public static JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions? firstOptions) =>
        firstOptions?.TryGetTypeInfo(type, out JsonTypeInfo? info) is true ?
            info :
            _defaultToolJsonOptions.GetTypeInfo(type);

    /// <summary>Creates the default <see cref="JsonSerializerOptions"/> to use for serialization-related operations.</summary>
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled")]
    private static JsonSerializerOptions CreateDefaultToolJsonOptions()
    {
        // If reflection-based serialization is enabled by default, use it, as it's the most permissive in terms of what it can serialize,
        // and we want to be flexible in terms of what can be put into the various collections in the object model.
        // Otherwise, use the source-generated options to enable trimming and Native AOT.

        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            // Keep in sync with the JsonSourceGenerationOptions attribute on JsonContext above.
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
            };

            options.MakeReadOnly();
            return options;
        }

        return Default.Options;
    }
}
