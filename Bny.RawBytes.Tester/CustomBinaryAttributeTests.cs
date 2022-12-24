namespace Bny.RawBytes.Tests;

[UnitTest]
internal class CustomBinaryAttributeTests
{
    [BinaryObject]
    class TestClass : IBinaryCustom
    {
        [CustomBinary(4)]
        public int Value { get; set; }

        public bool TryReadCustom(
            ReadOnlySpan<byte> data ,
            string?            id   ,
            BytesParam         param)
        {
            Value = Bytes.To<int>(data, param) + 5;
            return true;
        }

        public bool TryWriteCustom(Span<byte> output, string? id, BytesParam param)
        {
            Bytes.From(Value - 5, output, param);
            return true;
        }
    }

    [UnitTest]
    public static void Test_SpanTo(Asserter a)
    {
        Span<byte> data = new byte[] { 4, 0, 0, 0 };

        a.Assert(Bytes.TryTo<TestClass>(data, out var res) == 4);
        a.Assert(res.Value == 9);
    }

    [UnitTest]
    public static void Test_StreamTo(Asserter a)
    {
        MemoryStream data = new(new byte[] { 4, 0, 0, 0 });

        a.Assert(Bytes.TryTo<TestClass>(data, out var res));
        a.Assert(res.Value == 9);
    }

    [UnitTest]
    public static void Test_SpanFrom(Asserter a)
    {
        Span<byte> result = new byte[4];
        Span<byte> expectedResult = new byte[] { 4, 0, 0, 0 };
        TestClass data = new() { Value = 9 };

        a.Assert(Bytes.TryFrom(data, result) == 4);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_StreamFrom(Asserter a)
    {
        Span<byte> expectedResult = new byte[] { 4, 0, 0, 0 };
        TestClass data = new() { Value = 9 };
        using MemoryStream output = new();

        a.Assert(Bytes.TryFrom(data, output));
        a.Assert(expectedResult.StartsWith(output.ToArray()));
    }
}
