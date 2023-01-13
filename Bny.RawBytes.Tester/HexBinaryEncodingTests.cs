namespace Bny.RawBytes.Tests;

[UnitTest]
internal class HexBinaryEncodingTests
{
    [UnitTest]
    public static void Test_GetString(Asserter a)
    {
        Span<byte> data = new byte[] { 0x1A, 0xF5 };

        var res = Bytes.To<string>(data, new(Encoding: "hex"));

        a.Assert(res == "1AF5");
    }

    [UnitTest]
    public static void Test_GetBytes(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[] { 0x1A, 0xF5 };
        Span<byte> result = new byte[2];

        var res = Bytes.From("1AF5", result, new(Encoding: "hex"));

        a.Assert(res == 2);
        a.Assert(expectedResult.StartsWith(result));
    }
}
