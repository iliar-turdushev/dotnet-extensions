﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
#if TESTS_JSON_SCHEMA_EXPORTER_POLYFILL
using System.Text.Json.Schema;
#endif
using System.Text.Json.Serialization;
using System.Xml.Linq;

#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable JSON001 // Comments not allowed
#pragma warning disable S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes
#pragma warning disable SA1502 // Element should not be on a single line
#pragma warning disable SA1136 // Enum values should be on separate lines
#pragma warning disable SA1133 // Do not combine attributes
#pragma warning disable S3604 // Member initializer values should not be redundant
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions
#pragma warning disable IDE0073 // The file header is missing or not located at the top of the file

namespace Microsoft.Extensions.AI.JsonSchemaExporter;

public static partial class TestTypes
{
    public static IEnumerable<object[]> GetTestData() => GetTestDataCore().Select(t => new object[] { t });

    public static IEnumerable<object[]> GetTestDataUsingAllValues() =>
        GetTestDataCore()
        .SelectMany(t => t.GetTestDataForAllValues())
        .Select(t => new object[] { t });

    public static IEnumerable<ITestData> GetTestDataCore()
    {
        // Primitives and built-in types
        yield return new TestData<object>(
            Value: new(),
            AdditionalValues: [42, false, 3.14, 3.14M, new int[] { 1, 2, 3 }, new SimpleRecord(1, "str", false, 3.14)],
            ExpectedJsonSchema: "true");

        yield return new TestData<bool>(true, """{"type":"boolean"}""");
        yield return new TestData<byte>(42, """{"type":"integer"}""");
        yield return new TestData<ushort>(42, """{"type":"integer"}""");
        yield return new TestData<uint>(42, """{"type":"integer"}""");
        yield return new TestData<ulong>(42, """{"type":"integer"}""");
        yield return new TestData<sbyte>(42, """{"type":"integer"}""");
        yield return new TestData<short>(42, """{"type":"integer"}""");
        yield return new TestData<int>(42, """{"type":"integer"}""");
        yield return new TestData<long>(42, """{"type":"integer"}""");
        yield return new TestData<float>(1.2f, """{"type":"number"}""");
        yield return new TestData<double>(3.14159d, """{"type":"number"}""");
        yield return new TestData<decimal>(3.14159M, """{"type":"number"}""");
#if NET7_0_OR_GREATER
        yield return new TestData<UInt128>(42, """{"type":"integer"}""");
        yield return new TestData<Int128>(42, """{"type":"integer"}""");
#endif
#if NET6_0_OR_GREATER
        yield return new TestData<Half>((Half)3.141, """{"type":"number"}""");
#endif
        yield return new TestData<string>("I am a string", """{"type":["string","null"]}""");
        yield return new TestData<char>('c', """{"type":"string","minLength":1,"maxLength":1}""");
        yield return new TestData<byte[]>(
            Value: [1, 2, 3],
            AdditionalValues: [[]],
            ExpectedJsonSchema: """{"type":["string","null"]}""");

        yield return new TestData<Memory<byte>>(new byte[] { 1, 2, 3 }, """{"type":"string"}""");
        yield return new TestData<ReadOnlyMemory<byte>>(new byte[] { 1, 2, 3 }, """{"type":"string"}""");
        yield return new TestData<DateTime>(
            Value: new(2021, 1, 1),
            AdditionalValues: [DateTime.MinValue, DateTime.MaxValue],
            ExpectedJsonSchema: """{"type":"string","format": "date-time"}""");

        yield return new TestData<DateTimeOffset>(
            Value: new(new DateTime(2021, 1, 1), TimeSpan.Zero),
            AdditionalValues: [DateTimeOffset.MinValue, DateTimeOffset.MaxValue],
            ExpectedJsonSchema: """{"type":"string","format": "date-time"}""");

        yield return new TestData<TimeSpan>(
            Value: new(hours: 5, minutes: 13, seconds: 3),
            AdditionalValues: [TimeSpan.MinValue, TimeSpan.MaxValue],
            ExpectedJsonSchema: """{"$comment": "Represents a System.TimeSpan value.", "type":"string", "pattern": "^-?(\\d+\\.)?\\d{2}:\\d{2}:\\d{2}(\\.\\d{1,7})?$"}""");

#if NET6_0_OR_GREATER
        yield return new TestData<DateOnly>(new(2021, 1, 1), """{"type":"string","format": "date"}""");
        yield return new TestData<TimeOnly>(new(hour: 22, minute: 30, second: 33, millisecond: 100), """{"type":"string","format": "time"}""");
#endif
        yield return new TestData<Guid>(Guid.Empty, """{"type":"string","format":"uuid"}""");
        yield return new TestData<Uri>(new("http://example.com"), """{"type":["string","null"], "format":"uri"}""");
        yield return new TestData<Version>(new(1, 2, 3, 4), """{"$comment":"Represents a version string.", "type":["string","null"],"pattern":"^\\d+(\\.\\d+){1,3}$"}""");
        yield return new TestData<JsonDocument>(JsonDocument.Parse("""[{ "x" : 42 }]"""), "true");
        yield return new TestData<JsonElement>(JsonDocument.Parse("""[{ "x" : 42 }]""").RootElement, "true");
        yield return new TestData<JsonNode>(JsonNode.Parse("""[{ "x" : 42 }]"""), "true");
        yield return new TestData<JsonValue>((JsonValue)42, "true");
        yield return new TestData<JsonObject>(new() { ["x"] = 42 }, """{"type":["object","null"]}""");
        yield return new TestData<JsonArray>([(JsonNode)1, (JsonNode)2, (JsonNode)3], """{"type":["array","null"]}""");

        // Enum types
        yield return new TestData<IntEnum>(IntEnum.A, """{"type":"integer"}""");
        yield return new TestData<StringEnum>(StringEnum.A, """{"enum": ["A","B","C"]}""");
        yield return new TestData<FlagsStringEnum>(FlagsStringEnum.A, """{"type":"string"}""");

        // Nullable<T> types
        yield return new TestData<bool?>(true, """{"type":["boolean","null"]}""");
        yield return new TestData<int?>(42, """{"type":["integer","null"]}""");
        yield return new TestData<double?>(3.14, """{"type":["number","null"]}""");
        yield return new TestData<Guid?>(Guid.Empty, """{"type":["string","null"],"format":"uuid"}""");
        yield return new TestData<JsonElement?>(JsonDocument.Parse("{}").RootElement, "true");
        yield return new TestData<IntEnum?>(IntEnum.A, """{"type":["integer","null"]}""");
        yield return new TestData<StringEnum?>(StringEnum.A, """{"enum":["A","B","C",null]}""");
        yield return new TestData<SimpleRecordStruct?>(
            new(1, "two", true, 3.14),
            ExpectedJsonSchema: """
            {
                "type":["object","null"],
                "properties": {
                    "X": {"type":"integer"},
                    "Y": {"type":"string"},
                    "Z": {"type":"boolean"},
                    "W": {"type":"number"}
                }
            }
            """);

#if !NET9_0 && TESTS_JSON_SCHEMA_EXPORTER_POLYFILL
        // Regression test for https://github.com/dotnet/runtime/issues/117493
        yield return new TestData<int?>(
            Value: 42,
            AdditionalValues: [null],
            ExpectedJsonSchema: """{"type":["integer","null"]}""",
            ExporterOptions: new() { TreatNullObliviousAsNonNullable = true });

        yield return new TestData<DateTimeOffset?>(
            Value: DateTimeOffset.MinValue,
            AdditionalValues: [null],
            ExpectedJsonSchema: """{"type":["string","null"],"format":"date-time"}""",
            ExporterOptions: new() { TreatNullObliviousAsNonNullable = true });
#endif

        // User-defined POCOs
        yield return new TestData<SimplePoco>(
            Value: new() { String = "string", StringNullable = "string", Int = 42, Double = 3.14, Boolean = true },
            AdditionalValues: [new() { String = "str", StringNullable = null }],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "String": { "type": "string" },
                    "StringNullable": { "type": ["string", "null"] },
                    "Int": { "type": "integer" },
                    "Double": { "type": "number" },
                    "Boolean": { "type": "boolean" }
                }
            }
            """);

#if TESTS_JSON_SCHEMA_EXPORTER_POLYFILL
        // Same as above but with nullable types set to non-nullable
        yield return new TestData<SimplePoco>(
            Value: new() { String = "string", StringNullable = "string", Int = 42, Double = 3.14, Boolean = true },
            AdditionalValues: [new() { String = "str", StringNullable = null }],
            ExpectedJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "String": { "type": "string" },
                    "StringNullable": { "type": ["string", "null"] },
                    "Int": { "type": "integer" },
                    "Double": { "type": "number" },
                    "Boolean": { "type": "boolean" }
                }
            }
            """,
            ExporterOptions: new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });
#endif

        yield return new TestData<SimpleRecord>(
            Value: new(1, "two", true, 3.14),
            ExpectedJsonSchema: """
            {
              "type": ["object","null"],
              "properties": {
                "X": { "type": "integer" },
                "Y": { "type": "string" },
                "Z": { "type": "boolean" },
                "W": { "type": "number" }
              },
              "required": ["X","Y","Z","W"]
            }
            """);

        yield return new TestData<SimpleRecordStruct>(
            Value: new(1, "two", true, 3.14),
            ExpectedJsonSchema: """
            {
              "type": "object",
              "properties": {
                "X": { "type": "integer" },
                "Y": { "type": "string" },
                "Z": { "type": "boolean" },
                "W": { "type": "number" }
              }
            }
            """);

        yield return new TestData<RecordWithOptionalParameters>(
            Value: new(1, "two", true, 3.14, StringEnum.A),
            ExpectedJsonSchema: """
            {
              "type": ["object","null"],
              "properties": {
                "X1": { "type": "integer" },
                "X2": { "type": "string" },
                "X3": { "type": "boolean" },
                "X4": { "type": "number" },
                "X5": { "enum": ["A", "B", "C"] },
                "Y1": { "type": "integer", "default": 42 },
                "Y2": { "type": "string", "default": "str" },
                "Y3": { "type": "boolean", "default": true },
                "Y4": { "type": "number", "default": 0 },
                "Y5": { "enum": ["A", "B", "C"], "default": "A" }
              },
              "required": ["X1", "X2", "X3", "X4", "X5"]
            }
            """);

        yield return new TestData<PocoWithRequiredMembers>(
            new() { X = "str1", Y = "str2" },
            ExpectedJsonSchema: """
            {
              "type": ["object","null"],
              "properties": {
                "Y": { "type": "string" },
                "Z": { "type": "integer" },
                "X": { "type": "string" }
              },
              "required": [ "Y", "Z", "X" ]
            }
            """);

        yield return new TestData<PocoWithIgnoredMembers>(
            new() { X = 1, Y = 2 },
            ExpectedJsonSchema: """
            {
              "type": [ "object", "null" ],
              "properties": {
                "X": { "type": "integer" }
              }
            }
            """);
        yield return new TestData<PocoWithCustomNaming>(
            Value: new() { IntegerProperty = 1, StringProperty = "str" },
            ExpectedJsonSchema: """
            {
              "type": ["object","null"],
              "properties": {
                "int": { "type": "integer" },
                "str": { "type": [ "string", "null"] }
              }
            }
            """);

        yield return new TestData<PocoWithCustomNumberHandling>(
            Value: new() { X = 1 },
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": { "X": { "type": ["string","integer"], "pattern": "^-?(?:0|[1-9]\\d*)$" } }
            }
            """);

        yield return new TestData<PocoWithCustomNumberHandlingOnProperties>(
            Value: new() { X = 1, Y = 2, Z = 3 },
            AdditionalValues: [
                new() { X = 1, Y = double.NaN, Z = 3 },
                new() { X = 1, Y = double.PositiveInfinity, Z = 3 },
                new() { X = 1, Y = double.NegativeInfinity, Z = 3 },
            ],
            WritesNumbersAsStrings: true,
            ExpectedJsonSchema: """
            {
              "type": ["object","null"],
              "properties": {
                "X": { "type": ["string", "integer"], "pattern": "^-?(?:0|[1-9]\\d*)$" },
                "Y": {
                  "anyOf": [
                    { "type": "number" },
                    { "enum": ["NaN", "Infinity", "-Infinity"]}
                  ]
                },
                "Z": { "type": ["string", "integer"], "pattern": "^-?(?:0|[1-9]\\d*)$" },
                "W" : { "type": "number" }
              }
            }
            """);

        yield return new TestData<PocoWithRecursiveMembers>(
            Value: new() { Value = 1, Next = new() { Value = 2, Next = new() { Value = 3 } } },
            AdditionalValues: [new() { Value = 1, Next = null }],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "Value": { "type": "integer" },
                    "Next": {
                        "type": ["object","null"],
                        "properties": {
                            "Value": { "type": "integer" },
                            "Next": { "$ref": "#/properties/Next" }
                        }
                    }
                }
            }
            """);

#if TESTS_JSON_SCHEMA_EXPORTER_POLYFILL
        // Same as above but with non-nullable reference types by default.
        yield return new TestData<PocoWithRecursiveMembers>(
            Value: new() { Value = 1, Next = new() { Value = 2, Next = new() { Value = 3 } } },
            AdditionalValues: [new() { Value = 1, Next = null }],
            ExpectedJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "Value": { "type": "integer" },
                    "Next": {
                        "type": ["object","null"],
                        "properties": {
                            "Value": { "type": "integer" },
                            "Next": { "$ref": "#/properties/Next" }
                        }
                    }
                }
            }
            """,
            ExporterOptions: new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });

        SimpleRecord recordValue = new(42, "str", true, 3.14);
        yield return new TestData<PocoWithNonRecursiveDuplicateOccurrences>(
            Value: new() { Value1 = recordValue, Value2 = recordValue, ArrayValue = [recordValue], ListValue = [recordValue] },
            ExpectedJsonSchema: """
                {
                  "type": ["object","null"],
                  "properties": {
                    "Value1": {
                      "type": ["object","null"],
                      "properties": {
                        "X": { "type": "integer" },
                        "Y": { "type": "string" },
                        "Z": { "type": "boolean" },
                        "W": { "type": "number" }
                      },
                      "required": ["X", "Y", "Z", "W"]
                    },
                    /* The same type on a different property is repeated to
                       account for potential metadata resolved from attributes. */
                    "Value2": {
                      "type": ["object","null"],
                      "properties": {
                        "X": { "type": "integer" },
                        "Y": { "type": "string" },
                        "Z": { "type": "boolean" },
                        "W": { "type": "number" }
                      },
                      "required": ["X", "Y", "Z", "W"]
                    },
                    /* This collection element is the first occurrence
                       of the type without contextual metadata. */
                    "ListValue": {
                      "type": ["array","null"],
                      "items": {
                        "type": ["object","null"],
                        "properties": {
                          "X": { "type": "integer" },
                          "Y": { "type": "string" },
                          "Z": { "type": "boolean" },
                          "W": { "type": "number" }
                        },
                        "required": ["X", "Y", "Z", "W"]
                      }
                    },
                    /* This collection element is the second occurrence
                       of the type which points to the first occurrence. */
                    "ArrayValue": {
                      "type": ["array","null"],
                      "items": {
                        "$ref": "#/properties/ListValue/items"
                      }
                    }
                  }
                }
                """);
#endif

        yield return new TestData<PocoWithDescription>(
            Value: new() { X = 42 },
            ExpectedJsonSchema: """
            {
              "type": ["object","null"],
              "properties": {
                "X": {
                  "type": "integer"
                }
              }
            }
            """);

        yield return new TestData<PocoWithCustomConverter>(new() { Value = 42 }, "true");
        yield return new TestData<PocoWithCustomPropertyConverter>(new() { Value = 42 }, """{"type":["object","null"],"properties":{"Value":true}}""");
        yield return new TestData<PocoWithEnums>(
            Value: new()
            {
                IntEnum = IntEnum.A,
                StringEnum = StringEnum.B,
                IntEnumUsingStringConverter = IntEnum.A,
                NullableIntEnumUsingStringConverter = IntEnum.B,
                StringEnumUsingIntConverter = StringEnum.A,
                NullableStringEnumUsingIntConverter = StringEnum.B
            },
            AdditionalValues: [
                new()
                {
                    IntEnum = (IntEnum)int.MaxValue,
                    StringEnum = StringEnum.A,
                    IntEnumUsingStringConverter = IntEnum.A,
                    NullableIntEnumUsingStringConverter = null,
                    StringEnumUsingIntConverter = (StringEnum)int.MaxValue,
                    NullableStringEnumUsingIntConverter = null
                },
            ],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "IntEnum": { "type": "integer" },
                    "StringEnum": { "enum": [ "A", "B", "C" ] },
                    "IntEnumUsingStringConverter": { "enum": [ "A", "B", "C" ] },
                    "NullableIntEnumUsingStringConverter": { "enum": [ "A", "B", "C", null ] },
                    "StringEnumUsingIntConverter": { "type": "integer" },
                    "NullableStringEnumUsingIntConverter": { "type": [ "integer", "null" ] }
                }
            }
            """);

        var recordStruct = new SimpleRecordStruct(42, "str", true, 3.14);
        yield return new TestData<PocoWithStructFollowedByNullableStruct>(
            Value: new() { Struct = recordStruct, NullableStruct = null },
            AdditionalValues: [new() { Struct = recordStruct, NullableStruct = recordStruct }],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "Struct": {
                        "type": "object",
                        "properties": {
                            "X": {"type":"integer"},
                            "Y": {"type":"string"},
                            "Z": {"type":"boolean"},
                            "W": {"type":"number"}
                        }
                    },
                    "NullableStruct": {
                        "type": ["object","null"],
                        "properties": {
                            "X": {"type":"integer"},
                            "Y": {"type":"string"},
                            "Z": {"type":"boolean"},
                            "W": {"type":"number"}
                        }
                    }
                }
            }
            """);

        yield return new TestData<PocoWithNullableStructFollowedByStruct>(
            Value: new() { NullableStruct = null, Struct = recordStruct },
            AdditionalValues: [new() { NullableStruct = recordStruct, Struct = recordStruct }],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "NullableStruct": {
                        "type": ["object","null"],
                        "properties": {
                            "X": {"type":"integer"},
                            "Y": {"type":"string"},
                            "Z": {"type":"boolean"},
                            "W": {"type":"number"}
                        }
                    },
                    "Struct": {
                        "type": "object",
                        "properties": {
                            "X": {"type":"integer"},
                            "Y": {"type":"string"},
                            "Z": {"type":"boolean"},
                            "W": {"type":"number"}
                        }
                    }
                }
            }
            """);

        yield return new TestData<PocoWithExtensionDataProperty>(
            Value: new() { Name = "name", ExtensionData = new() { ["x"] = 42 } },
            """{"type":["object","null"],"properties":{"Name":{"type":["string","null"]}}}""");

        yield return new TestData<PocoDisallowingUnmappedMembers>(
            Value: new() { Name = "name", Age = 42 },
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "Name": {"type":["string","null"]},
                    "Age": {"type":"integer"}
                },
                "additionalProperties": false
            }
            """);

        // Global JsonUnmappedMemberHandling.Disallow setting
        yield return new TestData<SimplePoco>(
            Value: new() { String = "string", StringNullable = "string", Int = 42, Double = 3.14, Boolean = true },
            AdditionalValues: [new() { String = "str", StringNullable = null }],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "String": { "type": "string" },
                    "StringNullable": { "type": ["string", "null"] },
                    "Int": { "type": "integer" },
                    "Double": { "type": "number" },
                    "Boolean": { "type": "boolean" }
                },
                "additionalProperties": false
            }
            """,
            Options: new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow });

        yield return new TestData<PocoWithNullableAnnotationAttributes>(
            Value: new() { MaybeNull = null!, AllowNull = null, NotNull = null, DisallowNull = null!, NotNullDisallowNull = "str" },
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "MaybeNull": {"type":["string","null"]},
                    "AllowNull": {"type":["string","null"]},
                    "NotNull": {"type":["string","null"]},
                    "DisallowNull": {"type":["string","null"]},
                    "NotNullDisallowNull": {"type":"string"}
                }
            }
            """);

        yield return new TestData<PocoWithNullableAnnotationAttributesOnConstructorParams>(
            Value: new(allowNull: null, disallowNull: "str"),
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "AllowNull": {"type":["string","null"]},
                    "DisallowNull": {"type":"string"}
                },
                "required": ["AllowNull", "DisallowNull"]
            }
            """);

        yield return new TestData<PocoWithNullableConstructorParameter>(
            Value: new(null),
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "Value": {"type":["string","null"]}
                },
                "required": ["Value"]
            }
            """);

        yield return new TestData<PocoWithOptionalConstructorParams>(
            Value: new(),
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "X1": {"type":"string", "default": "str" },
                    "X2": {"type":"integer", "default": 42 },
                    "X3": {"type":"boolean", "default": true },
                    "X4": {"type":"number", "default": 0 },
                    "X5": {"enum":["A","B","C"], "default": "A" },
                    "X6": {"type":["string","null"], "default": "str" },
                    "X7": {"type":["integer","null"], "default": 42 },
                    "X8": {"type":["boolean","null"], "default": true },
                    "X9": {"type":["number","null"], "default": 0 },
                    "X10": {"enum":["A","B","C", null], "default": "A" }
                }
            }
            """);

        yield return new TestData<GenericPocoWithNullableConstructorParameter<string>>(
            Value: new(null!),
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "Value": {"type":["string","null"]}
                },
                "required": ["Value"]
            }
            """);

        yield return new TestData<PocoWithPolymorphism>(
            Value: new PocoWithPolymorphism.DerivedPocoStringDiscriminator { BaseValue = 42, DerivedValue = "derived" },
            AdditionalValues: [
                new PocoWithPolymorphism.DerivedPocoNoDiscriminator { BaseValue = 42, DerivedValue = "derived" },
                new PocoWithPolymorphism.DerivedPocoIntDiscriminator { BaseValue = 42, DerivedValue = "derived" },
                new PocoWithPolymorphism.DerivedCollection { BaseValue = 42 },
                new PocoWithPolymorphism.DerivedDictionary { BaseValue = 42 },
            ],

            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "anyOf": [
                    {
                        "properties": {
                            "BaseValue": {"type":"integer"},
                            "DerivedValue": {"type":["string","null"]}
                        }
                    },
                    {
                        "properties": {
                            "$type": {"const":"derivedPoco"},
                            "BaseValue": {"type":"integer"},
                            "DerivedValue": {"type":["string","null"]}
                        },
                        "required": ["$type"]
                    },
                    {
                        "properties": {
                            "$type": {"const":42},
                            "BaseValue": {"type":"integer"},
                            "DerivedValue": {"type":["string","null"]}
                        },
                        "required": ["$type"]
                    },
                    {
                        "properties": {
                            "$type": {"const":"derivedCollection"},
                            "$values": {
                                "type": "array",
                                "items": {"type":"integer"}
                            }
                        },
                        "required": ["$type"]
                    },
                    {
                        "properties": {
                            "$type": {"const":"derivedDictionary"}
                        },
                        "additionalProperties":{"type": "integer"},
                        "required": ["$type"]
                    }
                ]
            }
            """);

        yield return new TestData<NonAbstractClassWithSingleDerivedType>(
            Value: new NonAbstractClassWithSingleDerivedType(),
            AdditionalValues: [new NonAbstractClassWithSingleDerivedType.Derived()],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"]
            }
            """);

#if !NET9_0 // Disable until https://github.com/microsoft/semantic-kernel/issues/8983 gets backported to .NET 9
        yield return new TestData<ClassWithOptionalObjectParameter>(
            Value: new(value: null),
            AdditionalValues: [new(true), new(42), new(""), new(new object()), new(Array.Empty<int>())],
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                  "Value": { "default": null }
                }
            }
            """);
#endif

        yield return new TestData<PocoCombiningPolymorphicTypeAndDerivedTypes>(
            Value: new(),
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "PolymorphicValue": {
                        "type": "object",
                        "anyOf": [
                            {
                                "properties": {
                                    "BaseValue": {"type":"integer"},
                                    "DerivedValue": {"type":["string","null"]}
                                }
                            },
                            {
                                "properties": {
                                    "$type": {"const":"derivedPoco"},
                                    "BaseValue": {"type":"integer"},
                                    "DerivedValue": {"type":["string","null"]}
                                },
                                "required": ["$type"]
                            },
                            {
                                "properties": {
                                    "$type": {"const":42},
                                    "BaseValue": {"type":"integer"},
                                    "DerivedValue": {"type":["string","null"]}
                                },
                                "required": ["$type"]
                            },
                            {
                                "properties": {
                                    "$type": {"const":"derivedCollection"},
                                    "$values": {
                                        "type": "array",
                                        "items": {"type":"integer"}
                                    }
                                },
                                "required": ["$type"]
                            },
                            {
                                "properties": {
                                    "$type": {"const":"derivedDictionary"}
                                },
                                "additionalProperties":{"type": "integer"},
                                "required": ["$type"]
                            }
                        ]
                    },
                    "DerivedValue1": { 
                        "type": "object",
                        "properties": {
                            "BaseValue": {
                                "type": "integer"
                            },
                            "DerivedValue": {
                                "type": [
                                    "string",
                                    "null"
                                ]
                            }
                        }
                    },
                    "DerivedValue2": {
                        "type": "object",
                        "properties": {
                            "BaseValue": {"type":"integer"},
                            "DerivedValue": {"type":["string","null"]}
                        }
                    }
                }
            }
            """);

#if TEST
        yield return new TestData<ClassWithComponentModelAttributes>(
            Value: new("string", -1),
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "properties": {
                    "StringValue": {"type":"string","pattern":"\\w+"},
                    "IntValue": {"type":"integer","default":42}
                },
                "required": ["StringValue","IntValue"]
            }
            """,
            ExporterOptions: new JsonSchemaExporterOptions
            {
                TransformSchemaNode = static (ctx, schema) =>
                {
                    if (ctx.PropertyInfo is null || schema is not JsonObject jObj)
                    {
                        return schema;
                    }

                    if (ctx.ResolveAttribute<DefaultValueAttribute>() is { } attr)
                    {
                        jObj["default"] = JsonSerializer.SerializeToNode(attr.Value);
                    }

                    if (ctx.ResolveAttribute<RegularExpressionAttribute>() is { } regexAttr)
                    {
                        jObj["pattern"] = regexAttr.Pattern;
                    }

                    return jObj;
                }
            });
#endif

        // Collection types
        yield return new TestData<int[]>([1, 2, 3], """{"type":["array","null"],"items":{"type":"integer"}}""");
        yield return new TestData<List<bool>>([false, true, false], """{"type":["array","null"],"items":{"type":"boolean"}}""");
        yield return new TestData<HashSet<string>>(["one", "two", "three"], """{"type":["array","null"],"items":{"type":["string","null"]}}""");
        yield return new TestData<Queue<double>>(new([1.1, 2.2, 3.3]), """{"type":["array","null"],"items":{"type":"number"}}""");
        yield return new TestData<Stack<char>>(new(['x', '2', '+']), """{"type":["array","null"],"items":{"type":"string","minLength":1,"maxLength":1}}""");
        yield return new TestData<ImmutableArray<int>>(ImmutableArray.Create(1, 2, 3), """{"type":"array","items":{"type":"integer"}}""");
        yield return new TestData<ImmutableList<string>>(ImmutableList.Create("one", "two", "three"), """{"type":["array","null"],"items":{"type":["string","null"]}}""");
        yield return new TestData<ImmutableQueue<bool>>(ImmutableQueue.Create(false, false, true), """{"type":["array","null"],"items":{"type":"boolean"}}""");
        yield return new TestData<object[]>([1, "two", 3.14], """{"type":["array","null"]}""");
        yield return new TestData<System.Collections.ArrayList>([1, "two", 3.14], """{"type":["array","null"]}""");

        // Dictionary types
        yield return new TestData<Dictionary<string, int>>(
            Value: new() { ["one"] = 1, ["two"] = 2, ["three"] = 3 },
            ExpectedJsonSchema: """{"type":["object","null"],"additionalProperties":{"type": "integer"}}""");

        yield return new TestData<StructDictionary<string, int>>(
            Value: new([new("one", 1), new("two", 2), new("three", 3)]),
            ExpectedJsonSchema: """{"type":"object","additionalProperties":{"type": "integer"}}""");

        yield return new TestData<SortedDictionary<int, string>>(
            Value: new() { [1] = "one", [2] = "two", [3] = "three" },
            ExpectedJsonSchema: """{"type":["object","null"],"additionalProperties":{"type": ["string","null"]}}""");

        yield return new TestData<Dictionary<string, SimplePoco>>(
            Value: new()
            {
                ["one"] = new() { String = "string", StringNullable = "string", Int = 42, Double = 3.14, Boolean = true },
                ["two"] = new() { String = "string", StringNullable = null, Int = 42, Double = 3.14, Boolean = true },
                ["three"] = new() { String = "string", StringNullable = null, Int = 42, Double = 3.14, Boolean = true },
            },
            ExpectedJsonSchema: """
            {
                "type": ["object","null"],
                "additionalProperties": {
                    "properties": {
                        "String": { "type": "string" },
                        "StringNullable": { "type": ["string", "null"] },
                        "Int": { "type": "integer" },
                        "Double": { "type": "number" },
                        "Boolean": { "type": "boolean" }
                    },
                    "type": ["object","null"]
                }
            }
            """);

        yield return new TestData<Dictionary<string, object>>(
            Value: new() { ["one"] = 1, ["two"] = "two", ["three"] = 3.14 },
            ExpectedJsonSchema: """{"type":["object","null"]}""");

        yield return new TestData<Hashtable>(
            Value: new() { ["one"] = 1, ["two"] = "two", ["three"] = 3.14 },
            ExpectedJsonSchema: """{"type":["object","null"]}""");
    }

    public enum IntEnum { A, B, C }

    [JsonConverter(typeof(JsonStringEnumConverter<StringEnum>))]
    public enum StringEnum { A, B, C }

    [Flags, JsonConverter(typeof(JsonStringEnumConverter<FlagsStringEnum>))]
    public enum FlagsStringEnum { A = 1, B = 2, C = 4 }

    public class SimplePoco
    {
        public string String { get; set; } = "default";
        public string? StringNullable { get; set; }

        public int Int { get; set; }
        public double Double { get; set; }
        public bool Boolean { get; set; }
    }

    public record SimpleRecord(int X, string Y, bool Z, double W);
    public record struct SimpleRecordStruct(int X, string Y, bool Z, double W);

    public record RecordWithOptionalParameters(
        [property: Description("required integer")] int X1, string X2, bool X3, double X4, [Description("required string enum")] StringEnum X5,
        [property: Description("optional integer")] int Y1 = 42, string Y2 = "str", bool Y3 = true, double Y4 = 0, [Description("optional string enum")] StringEnum Y5 = StringEnum.A);

    public class PocoWithRequiredMembers
    {
        [JsonInclude]
        public required string X;

        public required string Y { get; set; }

        [JsonRequired]
        public int Z { get; set; }
    }

    public class PocoWithIgnoredMembers
    {
        public int X { get; set; }

        [JsonIgnore]
        public int Y { get; set; }
    }

    public class PocoWithCustomNaming
    {
        [JsonPropertyName("int")]
        public int IntegerProperty { get; set; }

        [JsonPropertyName("str")]
        public string? StringProperty { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class PocoWithCustomNumberHandling
    {
        public int X { get; set; }
    }

    public class PocoWithCustomNumberHandlingOnProperties
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int X { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
        public double Y { get; set; }

        [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
        public int Z { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
        public decimal W { get; set; }
    }

    public class PocoWithRecursiveMembers
    {
        public int Value { get; init; }
        public PocoWithRecursiveMembers? Next { get; init; }
    }

    public class PocoWithNonRecursiveDuplicateOccurrences
    {
        public SimpleRecord? Value1 { get; set; }
        public SimpleRecord? Value2 { get; set; }
        public List<SimpleRecord>? ListValue { get; set; }
        public SimpleRecord[]? ArrayValue { get; set; }
    }

    [Description("The type description")]
    public class PocoWithDescription
    {
        [Description("The property description")]
        public int X { get; set; }
    }

    [JsonConverter(typeof(CustomConverter))]
    public class PocoWithCustomConverter
    {
        public int Value { get; set; }

        public class CustomConverter : JsonConverter<PocoWithCustomConverter>
        {
            public override PocoWithCustomConverter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                new PocoWithCustomConverter { Value = reader.GetInt32() };

            public override void Write(Utf8JsonWriter writer, PocoWithCustomConverter value, JsonSerializerOptions options) =>
                writer.WriteNumberValue(value.Value);
        }
    }

    public class PocoWithCustomPropertyConverter
    {
        [JsonConverter(typeof(CustomConverter))]
        public int Value { get; set; }

        public class CustomConverter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => int.Parse(reader.GetString()!);

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString());
        }
    }

    public class PocoWithEnums
    {
        public IntEnum IntEnum { get; init; }
        public StringEnum StringEnum { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter<IntEnum>))]
        public IntEnum IntEnumUsingStringConverter { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter<IntEnum>))]
        public IntEnum? NullableIntEnumUsingStringConverter { get; set; }

        [JsonConverter(typeof(JsonNumberEnumConverter<StringEnum>))]
        public StringEnum StringEnumUsingIntConverter { get; set; }

        [JsonConverter(typeof(JsonNumberEnumConverter<StringEnum>))]
        public StringEnum? NullableStringEnumUsingIntConverter { get; set; }
    }

    public class PocoWithStructFollowedByNullableStruct
    {
        public SimpleRecordStruct? NullableStruct { get; set; }
        public SimpleRecordStruct Struct { get; set; }
    }

    public class PocoWithNullableStructFollowedByStruct
    {
        public SimpleRecordStruct? NullableStruct { get; set; }
        public SimpleRecordStruct Struct { get; set; }
    }

    public class PocoWithExtensionDataProperty
    {
        public string? Name { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class PocoDisallowingUnmappedMembers
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class PocoWithNullableAnnotationAttributes
    {
        [MaybeNull]
        public string MaybeNull { get; set; }

        [AllowNull]
        public string AllowNull { get; set; }

        [NotNull]
        public string? NotNull { get; set; }

        [DisallowNull]
        public string? DisallowNull { get; set; }

        [NotNull, DisallowNull]
        public string? NotNullDisallowNull { get; set; } = "";
    }

    public class PocoWithNullableAnnotationAttributesOnConstructorParams([AllowNull] string allowNull, [DisallowNull] string? disallowNull)
    {
        public string AllowNull { get; } = allowNull!;
        public string DisallowNull { get; } = disallowNull;
    }

    public class PocoWithNullableConstructorParameter(string? value)
    {
        public string Value { get; } = value!;
    }

    public class PocoWithOptionalConstructorParams(
        string x1 = "str", int x2 = 42, bool x3 = true, double x4 = 0, StringEnum x5 = StringEnum.A,
        string? x6 = "str", int? x7 = 42, bool? x8 = true, double? x9 = 0, StringEnum? x10 = StringEnum.A)
    {
        public string X1 { get; } = x1;
        public int X2 { get; } = x2;
        public bool X3 { get; } = x3;
        public double X4 { get; } = x4;
        public StringEnum X5 { get; } = x5;

        public string? X6 { get; } = x6;
        public int? X7 { get; } = x7;
        public bool? X8 { get; } = x8;
        public double? X9 { get; } = x9;
        public StringEnum? X10 { get; } = x10;
    }

    // Regression test for https://github.com/dotnet/runtime/issues/92487
    public class GenericPocoWithNullableConstructorParameter<T>(T value)
    {
        [NotNull]
        public T Value { get; } = value!;
    }

    [JsonDerivedType(typeof(DerivedPocoNoDiscriminator))]
    [JsonDerivedType(typeof(DerivedPocoStringDiscriminator), "derivedPoco")]
    [JsonDerivedType(typeof(DerivedPocoIntDiscriminator), 42)]
    [JsonDerivedType(typeof(DerivedCollection), "derivedCollection")]
    [JsonDerivedType(typeof(DerivedDictionary), "derivedDictionary")]
    public abstract class PocoWithPolymorphism
    {
        public int BaseValue { get; set; }

        public class DerivedPocoNoDiscriminator : PocoWithPolymorphism
        {
            public string? DerivedValue { get; set; }
        }

        public class DerivedPocoStringDiscriminator : PocoWithPolymorphism
        {
            public string? DerivedValue { get; set; }
        }

        public class DerivedPocoIntDiscriminator : PocoWithPolymorphism
        {
            public string? DerivedValue { get; set; }
        }

        public class DerivedCollection : PocoWithPolymorphism, IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => Enumerable.Repeat(BaseValue, 1).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class DerivedDictionary : PocoWithPolymorphism, IReadOnlyDictionary<string, int>
        {
            public int this[string key] => key == nameof(BaseValue) ? BaseValue : throw new KeyNotFoundException();
            public IEnumerable<string> Keys => [nameof(BaseValue)];
            public IEnumerable<int> Values => [BaseValue];
            public int Count => 1;
            public bool ContainsKey(string key) => key == nameof(BaseValue);
            public bool TryGetValue(string key, out int value) => key == nameof(BaseValue) ? (value = BaseValue) == BaseValue : (value = 0) == 0;
            public IEnumerator<KeyValuePair<string, int>> GetEnumerator() => Enumerable.Repeat(new KeyValuePair<string, int>(nameof(BaseValue), BaseValue), 1).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    [JsonDerivedType(typeof(NonAbstractClassWithSingleDerivedType.Derived))]
    public class NonAbstractClassWithSingleDerivedType
    {
        public class Derived : NonAbstractClassWithSingleDerivedType;
    }

    public class PocoCombiningPolymorphicTypeAndDerivedTypes
    {
        public PocoWithPolymorphism PolymorphicValue { get; set; } = new PocoWithPolymorphism.DerivedPocoNoDiscriminator { DerivedValue = "derived" };
        public PocoWithPolymorphism.DerivedPocoNoDiscriminator DerivedValue1 { get; set; } = new() { DerivedValue = "derived" };
        public PocoWithPolymorphism.DerivedPocoStringDiscriminator DerivedValue2 { get; set; } = new() { DerivedValue = "derived" };
    }

    public class ClassWithComponentModelAttributes
    {
        public ClassWithComponentModelAttributes(string stringValue, [DefaultValue(42)] int intValue)
        {
            StringValue = stringValue;
            IntValue = intValue;
        }

        [RegularExpression(@"\w+")]
        public string StringValue { get; }

        public int IntValue { get; }
    }

    public class ClassWithOptionalObjectParameter(object? value = null)
    {
        public object? Value { get; } = value;
    }

    public readonly struct StructDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> values)
        : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly IReadOnlyDictionary<TKey, TValue> _dictionary = values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        public TValue this[TKey key] => _dictionary[key];
        public IEnumerable<TKey> Keys => _dictionary.Keys;
        public IEnumerable<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
#if NET
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);
#else
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
#endif
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();
    }

    public class ClassWithPropertiesUsingCustomConverters
    {
        [JsonPropertyOrder(0)]
        public ClassWithCustomConverter1? Prop1 { get; set; }
        [JsonPropertyOrder(1)]
        public ClassWithCustomConverter2? Prop2 { get; set; }

        [JsonConverter(typeof(CustomConverter<ClassWithCustomConverter1>))]
        public class ClassWithCustomConverter1;

        [JsonConverter(typeof(CustomConverter<ClassWithCustomConverter2>))]
        public class ClassWithCustomConverter2;

        public sealed class CustomConverter<T> : JsonConverter<T>
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => default;

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
                => writer.WriteNullValue();
        }
    }

    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(byte))]
    [JsonSerializable(typeof(ushort))]
    [JsonSerializable(typeof(uint))]
    [JsonSerializable(typeof(ulong))]
    [JsonSerializable(typeof(sbyte))]
    [JsonSerializable(typeof(short))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(float))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(decimal))]
#if NET7_0_OR_GREATER
    [JsonSerializable(typeof(UInt128))]
    [JsonSerializable(typeof(Int128))]
#endif
#if NET6_0_OR_GREATER
    [JsonSerializable(typeof(Half))]
#endif
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(char))]
    [JsonSerializable(typeof(byte[]))]
    [JsonSerializable(typeof(Memory<byte>))]
    [JsonSerializable(typeof(ReadOnlyMemory<byte>))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(DateTimeOffset))]
    [JsonSerializable(typeof(TimeSpan))]
#if NET6_0_OR_GREATER
    [JsonSerializable(typeof(DateOnly))]
    [JsonSerializable(typeof(TimeOnly))]
#endif
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(Uri))]
    [JsonSerializable(typeof(Version))]
    [JsonSerializable(typeof(JsonDocument))]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(JsonNode))]
    [JsonSerializable(typeof(JsonValue))]
    [JsonSerializable(typeof(JsonObject))]
    [JsonSerializable(typeof(JsonArray))]
    // Enum types
    [JsonSerializable(typeof(IntEnum))]
    [JsonSerializable(typeof(StringEnum))]
    [JsonSerializable(typeof(FlagsStringEnum))]
    // Nullable<T> types
    [JsonSerializable(typeof(bool?))]
    [JsonSerializable(typeof(int?))]
    [JsonSerializable(typeof(double?))]
    [JsonSerializable(typeof(Guid?))]
    [JsonSerializable(typeof(JsonElement?))]
    [JsonSerializable(typeof(IntEnum?))]
    [JsonSerializable(typeof(StringEnum?))]
    [JsonSerializable(typeof(SimpleRecordStruct?))]
    [JsonSerializable(typeof(DateTimeOffset?))]
    // User-defined POCOs
    [JsonSerializable(typeof(SimplePoco))]
    [JsonSerializable(typeof(SimpleRecord))]
    [JsonSerializable(typeof(SimpleRecordStruct))]
    [JsonSerializable(typeof(RecordWithOptionalParameters))]
    [JsonSerializable(typeof(PocoWithRequiredMembers))]
    [JsonSerializable(typeof(PocoWithIgnoredMembers))]
    [JsonSerializable(typeof(PocoWithCustomNaming))]
    [JsonSerializable(typeof(PocoWithCustomNumberHandling))]
    [JsonSerializable(typeof(PocoWithCustomNumberHandlingOnProperties))]
    [JsonSerializable(typeof(PocoWithRecursiveMembers))]
    [JsonSerializable(typeof(PocoWithNonRecursiveDuplicateOccurrences))]
    [JsonSerializable(typeof(PocoWithDescription))]
    [JsonSerializable(typeof(PocoWithCustomConverter))]
    [JsonSerializable(typeof(PocoWithCustomPropertyConverter))]
    [JsonSerializable(typeof(PocoWithEnums))]
    [JsonSerializable(typeof(PocoWithStructFollowedByNullableStruct))]
    [JsonSerializable(typeof(PocoWithNullableStructFollowedByStruct))]
    [JsonSerializable(typeof(PocoWithExtensionDataProperty))]
    [JsonSerializable(typeof(PocoDisallowingUnmappedMembers))]
    [JsonSerializable(typeof(PocoWithNullableAnnotationAttributes))]
    [JsonSerializable(typeof(PocoWithNullableAnnotationAttributesOnConstructorParams))]
    [JsonSerializable(typeof(PocoWithNullableConstructorParameter))]
    [JsonSerializable(typeof(PocoWithOptionalConstructorParams))]
    [JsonSerializable(typeof(GenericPocoWithNullableConstructorParameter<string>))]
    [JsonSerializable(typeof(PocoWithPolymorphism))]
    [JsonSerializable(typeof(NonAbstractClassWithSingleDerivedType))]
    [JsonSerializable(typeof(PocoCombiningPolymorphicTypeAndDerivedTypes))]
    [JsonSerializable(typeof(ClassWithComponentModelAttributes))]
    [JsonSerializable(typeof(ClassWithOptionalObjectParameter))]
    [JsonSerializable(typeof(ClassWithPropertiesUsingCustomConverters))]
    // Collection types
    [JsonSerializable(typeof(int[]))]
    [JsonSerializable(typeof(List<bool>))]
    [JsonSerializable(typeof(HashSet<string>))]
    [JsonSerializable(typeof(Queue<double>))]
    [JsonSerializable(typeof(Stack<char>))]
    [JsonSerializable(typeof(ImmutableArray<int>))]
    [JsonSerializable(typeof(ImmutableList<string>))]
    [JsonSerializable(typeof(ImmutableQueue<bool>))]
    [JsonSerializable(typeof(object[]))]
    [JsonSerializable(typeof(System.Collections.ArrayList))]
    [JsonSerializable(typeof(Dictionary<string, int>))]
    [JsonSerializable(typeof(SortedDictionary<int, string>))]
    [JsonSerializable(typeof(Dictionary<string, SimplePoco>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(Hashtable))]
    [JsonSerializable(typeof(StructDictionary<string, int>))]
    [JsonSerializable(typeof(XElement))]
    public partial class TestTypesContext : JsonSerializerContext;
}
