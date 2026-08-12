# Json5.NET

A JSON5 reader for .NET. Parse config files with comments, trailing commas, unquoted keys and single quotes directly into `System.Text.Json` values. Zero external dependencies.

Config files get hand-edited, and JSON is a bad format for that: no comments, no trailing commas, every key wrapped in quotes. [JSON5](https://json5.org) fixes this by extending JSON with the parts of the ES5 object literal grammar people actually want, while staying a strict superset so any valid JSON is still valid JSON5. JavaScript has had a mature parser for it (`json5` on npm) for over a decade. .NET has never had one, and `System.Text.Json` [explicitly declined to add one](https://github.com/dotnet/runtime/issues/29804): the team's position is that JSON5 support belongs in a separate package, not the BCL. Json5.NET is that package: a strict, grammar-exact tokenizer, verified against the official JSON5 test corpus, that hands you back the same `JsonNode`/`JsonElement` types you already use.

## Install

```
dotnet add package Json5.Net
```

## Quickstart

Parse into a `JsonNode` tree:

```csharp
using Json5;

var config = Json5.Parse("""
    {
      // served on this port
      host: 'localhost',
      port: 8080,
      allowedOrigins: [
        'https://example.com',
        'https://app.example.com',
      ],
    }
    """);

Console.WriteLine(config!["port"]!.GetValue<int>());
// 8080
```

Deserialize straight into your own type:

```csharp
using Json5;

record ServerConfig(string Host, int Port, string[] AllowedOrigins);

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var config = Json5.Deserialize<ServerConfig>(File.ReadAllText("appsettings.json5"), options);
```

Handle malformed config with a real position, not a byte offset into a wall of text:

```csharp
using Json5;

try
{
    Json5.Parse(configText);
}
catch (Json5Exception ex)
{
    Console.WriteLine($"{configPath}:{ex.Line}:{ex.Column}: {ex.Message}");
}
```

`Json5Exception` derives from `System.Text.Json.JsonException`, so existing `catch (JsonException)` handlers keep working unchanged.

## What JSON5 adds over JSON

- `//` line comments and `/* */` block comments, anywhere insignificant whitespace is allowed
- Trailing commas in objects and arrays
- Unquoted object keys, using the ECMAScript `IdentifierName` grammar (letters, `$`, `_`, digits after the first character, `\uXXXX` escapes)
- Single-quoted keys and strings, alongside double-quoted
- String line continuations: a backslash immediately before a line break joins the line without inserting a newline
- Leading and trailing decimal points (`.5`, `5.`)
- An explicit leading `+` on numbers
- Hexadecimal integers (`0xDEADBEEF`)
- Signed `Infinity` and `NaN` as number literals

Json5.NET implements the full grammar above, including the parts that are easy to get subtly wrong: leading zeros are rejected exactly where ECMAScript rejects them (`010` and `080` both fail, `0`, `0.5` and `0x1A` all succeed), and reserved words like `while` are legal unquoted keys even though they aren't legal ES5 identifiers in value position.

## Zero dependencies, AOT-friendly

The library has no runtime NuGet dependencies. It parses hand-written text with its own tokenizer and hands the result to `System.Text.Json` types you already know: `JsonNode` for the tree, `JsonSerializer` under the hood for `Deserialize<T>`. No reflection of its own, no dynamic code generation, so it works unmodified under Native AOT and trimming, same as any other `System.Text.Json`-based code in your project.

## Verified against the official test corpus

Json5.NET's test suite embeds the full [json5-tests](https://github.com/json5/json5-tests) corpus (MIT licensed): every case marked valid is asserted to parse, and every case marked invalid is asserted to throw `Json5Exception` with a usable line and column. It also cross-checks against the JSON5 specification's own worked example and a JSON5/JSON document pair shipped in that corpus, asserting the two parse to identical trees.

## Notes and limitations

- `Infinity` and `NaN` have no representation in strict JSON text. `Json5.Parse` returns them as ordinary `JsonValue<double>` nodes, so reading them back with `GetValue<double>()` just works. `Json5.Deserialize<T>` always enables `JsonNumberHandling.AllowNamedFloatingPointLiterals` so they land correctly on `double`/`float` members too. If you later write that `JsonNode` tree back out as JSON text yourself, you will hit the same `System.Text.Json` restriction any other non-finite `double` does; that is standard JSON tooling behavior, not something specific to this library.
- Object/array nesting is capped at 64 levels, matching the `System.Text.Json` `JsonDocument` default, so a hostile deeply-nested document fails with a catchable `Json5Exception` rather than a `StackOverflowException`.
- Duplicate object keys are accepted, with the last value winning, matching both the JSON grammar (which does not forbid duplicates) and the reference JSON5 implementation's behavior.

## License

MIT. See [LICENSE](LICENSE). The embedded test fixtures under `tests/Json5.Net.Tests/fixtures/json5-tests` are the [json5-tests](https://github.com/json5/json5-tests) corpus, also MIT licensed, with its own [LICENSE.md](tests/Json5.Net.Tests/fixtures/json5-tests/LICENSE.md) preserved alongside it.
