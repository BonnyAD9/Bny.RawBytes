namespace Bny.RawBytes.Tests;

[UnitTest]
internal class TrimLargeDataTests
{
    [BinaryObject]
    class TestClass
    {
        [BinaryMember(trimLargeData: true, size: 5)]
        public string Value = "Hello World";
    }

    [UnitTest]
    public static void TestSpan(Asserter a)
    {
        Span<byte> result = new byte[5];
        ReadOnlySpan<byte> expectedResult = "Hello"u8;
        string data = "Hello World";

        Bytes.From(data, result, new(TrimLargeData: true));

        a.Assert(expectedResult.StartsWith(result));
    }

    [UnitTest]
    public static void TestAttributeSpan(Asserter a)
    {
        Span<byte> result = new byte[5];
        ReadOnlySpan<byte> expectedResult = "Hello"u8;
        string data = "Hello World";

        Bytes.From(data, result, new(TrimLargeData: true));

        a.Assert(expectedResult.StartsWith(result));
    }
}
