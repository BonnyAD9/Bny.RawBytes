using System.Security.Cryptography.X509Certificates;

namespace Bny.RawBytes.Tests;

[UnitTest]
internal class BinaryPaddingAttributeTests
{
    [BinaryObject]
    class TestClass
    {
        [BinaryMember]
        public int Value1 { get; set; }

        [BinaryPadding(size: 3)]
        [BinaryMember]
        public int Value2;

        [BinaryMember]
        [BinaryPadding(size: 5)]
        public int Value3 { get; set; }

        [BinaryMember]
        public int Value4 { get; set; }
    }

    [UnitTest]
    public static void Test_SpanTo(Asserter a)
    {
        var data = new byte[]
        {
            0, 0, 0, 1, // Value1
            0, 0, 0, // Padding
            0, 0, 0, 2, // Value2
            0, 0, 0, 3, // Value3
            0, 0, 0, 0, 0, // Padding
            0, 0, 0, 4, // Value4
        };

        TestClass result = Bytes.To<TestClass>(
            data,
            out int bytesReaded,
            new(Endianness: Endianness.Big));

        a.Assert(bytesReaded == data.Length);
        a.Assert(result.Value1 == 1);
        a.Assert(result.Value2 == 2);
        a.Assert(result.Value3 == 3);
        a.Assert(result.Value4 == 4);
    }

    [UnitTest]
    public static void Test_StreamTo(Asserter a)
    {
        MemoryStream data = new(new byte[]
        {
            0, 0, 0, 1, // Value1
            0, 0, 0, // Padding
            0, 0, 0, 2, // Value2
            0, 0, 0, 3, // Value3
            0, 0, 0, 0, 0, // Padding
            0, 0, 0, 4, // Value4
        }, false);

        TestClass result =
            Bytes.To<TestClass>(data, new(Endianness: Endianness.Big));

        a.Assert(result.Value1 == 1);
        a.Assert(result.Value2 == 2);
        a.Assert(result.Value3 == 3);
        a.Assert(result.Value4 == 4);
    }

    [UnitTest]
    public static void Test_SpanFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[]
        {
            0, 0, 0, 1, // Value1
            0, 0, 0, // Padding
            0, 0, 0, 2, // Value2
            0, 0, 0, 3, // Value3
            0, 0, 0, 0, 0, // Padding
            0, 0, 0, 4, // Value4
        };

        TestClass data = new()
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3,
            Value4 = 4,
        };

        Span<byte> result = new byte[24];

        int bytesWritten =
            Bytes.From(data, result, new(Endianness: Endianness.Big));

        a.Assert(bytesWritten == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_StreamFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[]
        {
            0, 0, 0, 1, // Value1
            0, 0, 0, // Padding
            0, 0, 0, 2, // Value2
            0, 0, 0, 3, // Value3
            0, 0, 0, 0, 0, // Padding
            0, 0, 0, 4, // Value4
        };

        TestClass data = new()
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3,
            Value4 = 4,
        };

        using (MemoryStream output = new())
        {
            Bytes.From(data, output, new(Endianness: Endianness.Big));
            ReadOnlySpan<byte> result = output.ToArray();

            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }
    }
}
