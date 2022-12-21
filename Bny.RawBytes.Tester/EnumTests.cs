namespace Bny.RawBytes.Tests;

[UnitTest]
internal class EnumTests
{
    enum TestEnum : short
    {
        TestValueOne = 1,
        TestValueFive = 5,
    }

    [UnitTest]
    public static void Test_SpanTo(Asserter a)
    {
        Span<byte> data = new byte[] { 5, 0 };

        a.Assert(Bytes.TryTo<TestEnum>(data, out var res) == 2);
        a.Assert(res == TestEnum.TestValueFive);
    }

    [UnitTest]
    public static void Test_StreamTo(Asserter a)
    {
        var data = new MemoryStream(new byte[] { 5, 0 }, false);

        a.Assert(Bytes.TryTo<TestEnum>(data, out var res));
        a.Assert(res == TestEnum.TestValueFive);
    }

    [UnitTest]
    public static void Test_SpanFrom(Asserter a)
    {
        Span<byte> result = new byte[2];
        Span<byte> expectedResult = new byte[] { 5, 0 };

        a.Assert(Bytes.TryFrom(TestEnum.TestValueFive, result) == 2);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_StreamFrom(Asserter a)
    {
        Span<byte> expectedResult = new byte[] { 5, 0 };

        using var output = new MemoryStream();
        a.Assert(Bytes.TryFrom(TestEnum.TestValueFive, output));
        Span<byte> result = output.ToArray();
        a.Assert(result.StartsWith(expectedResult));
    }
}
