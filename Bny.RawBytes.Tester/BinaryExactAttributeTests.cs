namespace Bny.RawBytes.Tests;

[UnitTest]
internal class BinaryExactAttributeTests
{
    [BinaryObject]
    class TestClass
    {
        [BinaryExact("ID3")]
        [BinaryMember]
        public string Value { get; set; }
    }

    [UnitTest]
    public static void Test_SpanTo(Asserter a)
    {
        ReadOnlySpan<byte> data1 = "ID3Artist"u8;
        ReadOnlySpan<byte> data2 = "TAGArtist"u8;

        var result = Bytes.To<TestClass>(data1, out int readedBytes);

        a.Assert(readedBytes == data1.Length);
        a.Assert(result.Value == "Artist");
        a.Assert(Bytes.TryTo<TestClass>(data2, out _) == -1);
    }

    [UnitTest]
    public static void Test_StreamTo(Asserter a)
    {
        MemoryStream data1 = new("ID3Artist"u8.ToArray(), false);
        MemoryStream data2 = new("TAGArtist"u8.ToArray(), false);

        var result = Bytes.To<TestClass>(data1);

        a.Assert(result.Value == "Artist");
        a.Assert(!Bytes.TryTo<TestClass>(data2, out _));
    }

    [UnitTest]
    public static void Test_SpanFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = "ID3Artist"u8;
        TestClass data = new() { Value = "Artist" };
        Span<byte> result = new byte[expectedResult.Length];

        var bytesWritten = Bytes.From(data, result);

        a.Assert(bytesWritten == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_StreamFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = "ID3Artist"u8;
        TestClass data = new() { Value = "Artist" };

        using (MemoryStream output = new())
        {
            Bytes.From(data, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }
    }
}
