namespace Bny.RawBytes.Tests;

[UnitTest]
internal class BinaryMemberAttributeSizeFromTests
{
    [BinaryObject]
    class TestClass
    {
        [BinaryMember(size: 5)]
        public string Value;
    }

    [UnitTest]
    public static void Test_Span(Asserter a)
    {
        TestClass data1 = new() { Value = "Hello" };
        TestClass data2 = new() { Value = "Hel" };
        TestClass data3 = new() { Value = "Hello World" };

        ReadOnlySpan<byte> expectedResult1 = "Hello"u8;
        ReadOnlySpan<byte> expectedResult2 = "Hel\0\0"u8;

        Span<byte> result = new byte[10];

        a.Assert(Bytes.From(data1, result) == 5);
        a.Assert(result.StartsWith(expectedResult1));
        a.Assert(Bytes.From(data2, result) == 5);
        a.Assert(result.StartsWith(expectedResult2));
        //a.Assert(Bytes.From(data3, result) == 5);
        //a.Assert(result.StartsWith(expectedResult1));
    }

    [UnitTest]
    public static void Test_Stream(Asserter a)
    {
        TestClass data1 = new() { Value = "Hello" };
        TestClass data2 = new() { Value = "Hel" };
        TestClass data3 = new() { Value = "Hello World" };

        ReadOnlySpan<byte> expectedResult1 = "Hello"u8;
        ReadOnlySpan<byte> expectedResult2 = "Hel\0\0"u8;

        using (MemoryStream output = new())
        {
            Bytes.From(data1, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == 5);
            a.Assert(result.StartsWith(expectedResult1));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(data2, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == 5);
            a.Assert(result.StartsWith(expectedResult2));
        }

        /*using (MemoryStream output = new())
        {
            Bytes.From(data3, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == 5);
            a.Assert(result.StartsWith(expectedResult1));
        }*/
    }
}
