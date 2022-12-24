using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Bny.RawBytes.Tests;

[UnitTest]
internal class CustomBinaryAttributeTests
{
    class TestBinAttribute : CustomBinaryAttribute
    {
        public TestBinAttribute([CallerLineNumber] int order = 0)
            : base(order) { }

        public override int ReadFromSpan(ReadOnlySpan<byte> span, out object? obj, BytesParam param)
        {
            var ret = Bytes.TryTo(span, out int i, param);
            obj = i + 5;
            return ret;
        }

        public override bool ReadFromStream(Stream stream, [NotNullWhen(true)] out object? obj, BytesParam param)
        {
            var ret = Bytes.TryTo(stream, out int i, param);
            obj = i + 5;
            return ret;
        }

        public override int WriteToSpan(object? obj, Span<byte> output, BytesParam param)
        {
            return Bytes.TryFrom((int)obj! - 5, output, param);
        }

        public override bool WriteToStream(object? obj, Stream output, BytesParam param)
        {
            return Bytes.TryFrom((int)obj! - 5, output, param);
        }
    }

    [BinaryObject]
    class TestClass
    {
        [TestBin]
        public int Value { get; set; }
    }

    [UnitTest]
    public static void Test_SpanTo(Asserter a)
    {
        Span<byte> data = new byte[] { 4, 0, 0, 0 }; // little endian

        a.Assert(Bytes.TryTo(data, out TestClass? res) == 4);
        a.Assert(res?.Value == 9);
    }

    [UnitTest]
    public static void Test_StreamTo(Asserter a)
    {
        MemoryStream data = new(new byte[] { 4, 0, 0, 0 }, false);

        a.Assert(Bytes.TryTo(data, out TestClass? res));
        a.Assert(res?.Value == 9);
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
