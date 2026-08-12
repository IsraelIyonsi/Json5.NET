# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-08-12

### Added

- `Json5` static API: `Parse(string)` returning a `System.Text.Json.Nodes.JsonNode?`, `TryParse(string?, out JsonNode?)` non-throwing variant, and `Deserialize<T>(string, JsonSerializerOptions?)` that parses JSON5 and hands the result to `System.Text.Json.JsonSerializer`.
- Full JSON5 (json5.org) grammar support in a hand-written tokenizer and recursive-descent parser: line and block comments, trailing commas in objects and arrays, unquoted object keys per the ECMAScript `IdentifierName` grammar (including reserved words and `\uXXXX` escapes), single-quoted keys and strings, string line continuations, leading and trailing decimal points, an explicit leading `+` on numbers, hexadecimal integers, signed `Infinity`/`NaN`, and the ECMAScript leading-zero restrictions on decimal literals.
- `Json5Exception`, deriving from `System.Text.Json.JsonException`, carrying the one-based `Line` and `Column` of the character where parsing failed.
- A nesting depth guard (64 levels, matching the `System.Text.Json` `JsonDocument` default) so deeply nested input fails with a catchable `Json5Exception` instead of a `StackOverflowException`.
- `Json5.Deserialize<T>` enables `JsonNumberHandling.AllowNamedFloatingPointLiterals` by default so JSON5 `Infinity`/`NaN` literals deserialize correctly into `double`/`float` members.
- Test suite embeds the official [json5-tests](https://github.com/json5/json5-tests) corpus (MIT licensed) as fixtures: every valid case is asserted to parse and every invalid case is asserted to throw `Json5Exception` with a usable position, plus dedicated oracle tests against the corpus's JSON5/JSON equivalence pair and the JSON5 specification's own worked example.
- Zero runtime dependencies; built on the in-box `System.Text.Json`.
- SourceLink (GitHub), deterministic CI builds and `.snupkg` symbol packages.
