namespace Json5.Tests.Corpus;

/// <summary>
/// Discovers the embedded json5-tests corpus (https://github.com/json5/json5-tests, MIT
/// licensed) and classifies each file by the expectation its extension encodes, per the
/// corpus README: .json and .json5 files must parse; .js and .txt files must fail.
/// </summary>
public static class Json5CorpusFixtures
{
    private static readonly string RootDirectory =
        Path.Combine(AppContext.BaseDirectory, "fixtures", "json5-tests");

    private static readonly string[] ValidExtensions = [".json", ".json5"];
    private static readonly string[] InvalidExtensions = [".js", ".txt"];

    public static IEnumerable<object[]> ValidCases() =>
        EnumerateFiles(ValidExtensions).Select(RelativeCase);

    public static IEnumerable<object[]> InvalidCases() =>
        EnumerateFiles(InvalidExtensions).Select(RelativeCase);

    private static object[] RelativeCase(string absolutePath) =>
        [Path.GetRelativePath(RootDirectory, absolutePath), absolutePath];

    private static IEnumerable<string> EnumerateFiles(string[] extensions) =>
        Directory.EnumerateFiles(RootDirectory, "*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.Ordinal))
            .OrderBy(f => f, StringComparer.Ordinal);
}
