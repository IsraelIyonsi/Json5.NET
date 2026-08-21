using Json5;

namespace Json5.Tests.Corpus;

/// <summary>
/// Runs the full embedded json5-tests corpus (https://github.com/json5/json5-tests, MIT
/// licensed) through <see cref="Json5Convert.Parse(string)"/>. Every valid case must parse and every
/// invalid case must throw <see cref="Json5Exception"/> with a usable source position, exactly
/// as the corpus README specifies.
/// </summary>
public sealed class Json5CorpusTests
{
    [Theory]
    [MemberData(nameof(Json5CorpusFixtures.ValidCases), MemberType = typeof(Json5CorpusFixtures))]
    public void ValidCorpusFixture_Parses(string name, string path)
    {
        string text = File.ReadAllText(path);

        var exception = Record.Exception(() => Json5Convert.Parse(text));

        Assert.True(exception is null, $"'{name}' should parse but threw: {exception}");
    }

    [Theory]
    [MemberData(nameof(Json5CorpusFixtures.InvalidCases), MemberType = typeof(Json5CorpusFixtures))]
    public void InvalidCorpusFixture_ThrowsWithUsablePosition(string name, string path)
    {
        string text = File.ReadAllText(path);
        int lineCount = Math.Max(1, text.Split('\n').Length);

        var exception = Assert.Throws<Json5Exception>(() => Json5Convert.Parse(text));

        Assert.True(exception.Line >= 1, $"'{name}': line should be one-based, was {exception.Line}.");
        Assert.True(
            exception.Line <= lineCount,
            $"'{name}': reported line {exception.Line} exceeds the {lineCount} lines in the source.");
        Assert.True(exception.Column >= 1, $"'{name}': column should be one-based, was {exception.Column}.");
        Assert.False(string.IsNullOrWhiteSpace(exception.Message));
    }
}
