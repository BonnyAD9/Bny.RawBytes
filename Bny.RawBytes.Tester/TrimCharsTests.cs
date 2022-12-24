namespace Bny.RawBytes.Tests;

[UnitTest]
internal class TrimCharsTests
{
    [UnitTest]
    public static void Test_SpanTo(Asserter a)
    {
        var data = "  Hello \t"u8;
        a.Assert(Bytes.To<string>(data, new(TrimChars: " \t")) == "Hello");
    }

    [UnitTest]
    public static void Test_StreamTo(Asserter a)
    {
        MemoryStream data = new("  Hello \t"u8.ToArray(), false);
        a.Assert(Bytes.To<string>(data, new(TrimChars: " \t")) == "Hello");
    }

    [UnitTest]
    public static void Test_SpanFrom(Asserter a)
    {
        Span<byte> result = new byte[5];
        var expectedResult = "Hello"u8;
        var data = "  Hello \t";

        a.Assert(Bytes.TryFrom(data, result, new(TrimChars: " \t")) == 5);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_StreamFrom(Asserter a)
    {
        var expectedResult = "Hello"u8;
        var data = "  Hello \t";
        using MemoryStream result = new();

        a.Assert(Bytes.TryFrom(data, result, new(TrimChars: " \t")));
        a.Assert(expectedResult.StartsWith(result.ToArray()));
    }
}
