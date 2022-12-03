using System.Numerics;
using System.Text;

namespace Bny.RawBytes.Tests;

[UnitTest]
internal class BytesTests
{
    class SimpleIBOClass : IBinaryObject<SimpleIBOClass>
    {
        public static int ReadSize => 4;

        public int Value { get; set; }

        public int WriteSize => ReadSize;

        public static int TryReadFromBinary(
            ReadOnlySpan<byte> data                   ,
            out SimpleIBOClass? result                ,
            Endianness endianness = Endianness.Default)
        {
            result = new()
            {
                Value = Bytes.To<int>(
                    data                       ,
                    out int readedBytes        ,
                    new(Endianness: endianness))
            };
            return readedBytes;
        }

        public int TryWriteToBinary(Span<byte> data, Endianness endianness)
            => Bytes.From(Value, data, new(Endianness: endianness));
    }

    [BinaryObject]
    class SimpleBOAClass
    {
        [BinaryMember]
        public int Value { get; set; }
    }

    [BinaryObject]
    class BOAClass
    {
        [BinaryMember]
        public int Property { get; set; }

        [BinaryMember(endianness: Endianness.Big)]
        public ushort field;

        [BinaryMember(nullTerminated: true)]
        public string? Utf8String { get; set; }

        [BinaryMember(nullTerminated: true, encoding: "utf-16BE")]
        public string? Utf16String { get; set; }

        [BinaryMember]
        public SimpleBOAClass? SimpleBOA { get; set; }

        [BinaryMember]
        public SimpleIBOClass? SimpleIBO { get; set; }

        [BinaryMember(size: 5)]
        public BigInteger BInteger24 { get; set; }
    }

    [UnitTest]
    public static void Test_Defaults(Asserter a)
    {
        a.Assert(Bytes.IsDefaultLE == BitConverter.IsLittleEndian);
        
        Endianness endianness = BitConverter.IsLittleEndian
            ? Endianness.Little
            : Endianness.Big;

        a.Assert(Bytes.DefaultEndianness == endianness);
    }

    [UnitTest]
    public static void Test_SpanIntBasicTo(Asserter a)
    {
        ReadOnlySpan<byte> data = new byte[]
        {
            0, 0, 2, 0, 255, 255, 255, 255
        };

        int leResult = 0x20000;
        int beResult = 0x200;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<int>(data) == defaultResult);
        a.Assert(Bytes.To<int>(data, out int readedBytes) == defaultResult);
        a.Assert(readedBytes == sizeof(int));
        a.Assert((int)Bytes.To(data, typeof(int)) == defaultResult);

        a.Assert((int)Bytes.To(
            data           ,
            typeof(int)    ,
            out readedBytes) == defaultResult);

        a.Assert(readedBytes == sizeof(int));

        a.Assert(Bytes.TryTo(data, out int result) == sizeof(int));
        a.Assert(result == defaultResult);

        a.Assert(Bytes.TryTo(
            data                 ,
            typeof(int)          ,
            out object? objResult) == sizeof(int));

        a.Assert((int)objResult! == defaultResult);

        a.Assert(Bytes.To<int>(data, new(Endianness.Little)) == leResult);
        a.Assert(Bytes.To<int>(data, new(Endianness.Big)) == beResult);

        a.Assert(Bytes.To<int>(
            data                   ,
            new(Endianness.Default)) == defaultResult);
    }

    [UnitTest]
    public static void Test_SpanStringBasicTo(Asserter a)
    {
        a.Assert(Bytes.To<string>("Hello\0World"u8) == "Hello\0World");

        a.Assert(Bytes.To<string>(
            "Hello\0World"u8         ,
            out int readedBytes      ,
            new(NullTerminated: true)) == "Hello");

        a.Assert(readedBytes == "Hello\0".Length);

        var utf16Bytes = Encoding.Unicode.GetBytes("Hello\0World");

        a.Assert(Bytes.To<string>(
            utf16Bytes             ,
            new(Encoding: "utf-16")) == "Hello\0World");

        a.Assert(Bytes.To<string>(
            utf16Bytes                                   ,
            out readedBytes                              ,
            new(Encoding: "utf-16", NullTerminated: true)) == "Hello");
        
        a.Assert(readedBytes == "Hello\0".Length * 2);
    }

    [UnitTest]
    public static void Test_SpanIBinaryIntegerTo(Asserter a)
    {
        ReadOnlySpan<byte> data = new byte[] { 0, 0, 2, 0, 0, 0};
        int beResult = 0x2000000;
        int leResult = 0x20000;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<BigInteger>(data) == defaultResult);

        a.Assert(Bytes.To<BigInteger>(
            data                              ,
            new(Endianness: Endianness.Little)) == leResult);

        a.Assert(Bytes.To<BigInteger>(
            data                           ,
            new(Endianness: Endianness.Big)) == beResult);

        a.Assert(Bytes.To<BigInteger>(
            data                               ,
            new(Endianness: Endianness.Default)) == defaultResult);

        data = new byte[] { 255, 255, 255, 255 };

        a.Assert(Bytes.To<BigInteger>(data) == -1);
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Default)) == -1);
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Signed)) == -1);

        a.Assert(Bytes.To<BigInteger>(
            data                    ,
            new(Sign: Sign.Unsigned)) == uint.MaxValue);
    }

    [UnitTest]
    public static void Test_SpanIBinaryObjectTo(Asserter a)
    {
        ReadOnlySpan<byte> data = new byte[]
        {
            0, 0, 2, 0, 255, 255, 255, 255
        };

        int leResult = 0x20000;
        int beResult = 0x200;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<SimpleIBOClass>(data).Value == defaultResult);

        a.Assert(Bytes.To<SimpleIBOClass>(
            data                               ,
            new(Endianness: Endianness.Default)).Value == defaultResult);

        a.Assert(Bytes.To<SimpleIBOClass>(
            data                              ,
            new(Endianness: Endianness.Little)).Value == leResult);

        a.Assert(Bytes.To<SimpleIBOClass>(
            data                           ,
            new(Endianness: Endianness.Big)).Value == beResult);
    }

    [UnitTest]
    public static void Test_SpanAttributesTo(Asserter a)
    {
        var data = new byte[]
        {
            0, 0, 2, 0, // Property: 0x20000
            0, 2, // field: 2
            0x48, 0x65, 0x6c, 0x6c, 0x6f, 0, // Utf8String: Hello
            // Utf16String: World
            0x00, 0x57, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0, 0,
            255, 255, 255, 255, // SimpleBoa.Value: -1
            123, 0, 0, 0, // SimpleIBO.Value: 123
            0, 0, 1, 0, 0, // BInteger24: 0x10000
            255, 255, 255, 255, 255, // Some additional data
        };

        var result = Bytes.To<BOAClass>(
            data                              ,
            out int readedBytes               ,
            new(Endianness: Endianness.Little));

        a.Assert(readedBytes == data.Length - 5);
        a.Assert(result.Property == 0x20000);
        a.Assert(result.field == 2);
        a.Assert(result.Utf8String == "Hello");
        a.Assert(result.Utf16String == "World");
        a.Assert(result.SimpleBOA is not null);
        a.Assert(result.SimpleBOA!.Value == -1);
        a.Assert(result.SimpleIBO is not null);
        a.Assert(result.SimpleIBO!.Value == 123);
        a.Assert(result.BInteger24 == 0x10000);
    }

    [UnitTest]
    public static void Test_StreamIntBasicTo(Asserter a)
    {
        MemoryStream data = new(new byte[]
        {
            0, 0, 2, 0, 255, 255, 255, 255
        }, false);

        int leResult = 0x20000;
        int beResult = 0x200;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<int>(data) == defaultResult);
        data.Position = 0;
        a.Assert((int)Bytes.To(data, typeof(int)) == defaultResult);

        data.Position = 0;
        a.Assert(Bytes.TryTo(data, out int result));
        a.Assert(result == defaultResult);
        data.Position = 0;
        a.Assert(Bytes.TryTo(data, typeof(int), out object? objResult));
        a.Assert((int)objResult! == defaultResult);

        data.Position = 0;
        a.Assert(Bytes.To<int>(data, new(Endianness.Little)) == leResult);
        data.Position = 0;
        a.Assert(Bytes.To<int>(data, new(Endianness.Big)) == beResult);

        data.Position = 0;
        a.Assert(Bytes.To<int>(
            data                   ,
            new(Endianness.Default)) == defaultResult);
    }

    [UnitTest]
    public static void Test_StreamStringBasicTo(Asserter a)
    {
        MemoryStream data = new("Hello\0World"u8.ToArray(), false);

        a.Assert(Bytes.To<string>(data) == "Hello\0World");
        data.Position = 0;
        a.Assert(Bytes.To<string>(
            data                     ,
            new(NullTerminated: true)) == "Hello");

        MemoryStream utf16Bytes =
            new(Encoding.Unicode.GetBytes("Hello\0World"), false);

        a.Assert(Bytes.To<string>(
            utf16Bytes             ,
            new(Encoding: "utf-16")) == "Hello\0World");

        utf16Bytes.Position = 0;
        a.Assert(Bytes.To<string>(
            utf16Bytes                                   ,
            new(Encoding: "utf-16", NullTerminated: true)) == "Hello");
    }

    [UnitTest]
    public static void Test_StreamIBinaryIntegerTo(Asserter a)
    {
        MemoryStream data = new(new byte[] { 0, 0, 2, 0, 0, 0}, false);
        int beResult = 0x2000000;
        int leResult = 0x20000;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<BigInteger>(data) == defaultResult);

        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(
            data                              ,
            new(Endianness: Endianness.Little)) == leResult);

        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(
            data                           ,
            new(Endianness: Endianness.Big)) == beResult);

        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(
            data                               ,
            new(Endianness: Endianness.Default)) == defaultResult);

        data = new(new byte[] { 255, 255, 255, 255 }, false);

        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(data) == -1);
        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Default)) == -1);
        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Signed)) == -1);

        data.Position = 0;
        a.Assert(Bytes.To<BigInteger>(
            data                    ,
            new(Sign: Sign.Unsigned)) == uint.MaxValue);
    }

    [UnitTest]
    public static void Test_StreamIBinaryObjectTo(Asserter a)
    {
        MemoryStream data = new(new byte[]
        {
            0, 0, 2, 0, 255, 255, 255, 255
        }, false);

        int leResult = 0x20000;
        int beResult = 0x200;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<SimpleIBOClass>(data).Value == defaultResult);

        data.Position = 0;
        a.Assert(Bytes.To<SimpleIBOClass>(
            data                               ,
            new(Endianness: Endianness.Default)).Value == defaultResult);

        data.Position = 0;
        a.Assert(Bytes.To<SimpleIBOClass>(
            data                              ,
            new(Endianness: Endianness.Little)).Value == leResult);

        data.Position = 0;
        a.Assert(Bytes.To<SimpleIBOClass>(
            data                           ,
            new(Endianness: Endianness.Big)).Value == beResult);
    }

    [UnitTest]
    public static void Test_StreamAttributesTo(Asserter a)
    {
        MemoryStream data = new(new byte[]
        {
            0, 0, 2, 0, // Property: 0x20000
            0, 2, // field: 2
            0x48, 0x65, 0x6c, 0x6c, 0x6f, 0, // Utf8String: Hello
            // Utf16String: World
            0x00, 0x57, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0, 0,
            255, 255, 255, 255, // SimpleBoa.Value: -1
            123, 0, 0, 0, // SimpleIBO.Value: 123
            0, 0, 1, 0, 0, // BInteger24: 0x10000
            255, 255, 255, 255, 255, // Some additional data
        }, false);

        var result = Bytes.To<BOAClass>(
            data,
            new(Endianness: Endianness.Little));

        a.Assert(result.Property == 0x20000);
        a.Assert(result.field == 2);
        a.Assert(result.Utf8String == "Hello");
        a.Assert(result.Utf16String == "World");
        a.Assert(result.SimpleBOA is not null);
        a.Assert(result.SimpleBOA!.Value == -1);
        a.Assert(result.SimpleIBO is not null);
        a.Assert(result.SimpleIBO!.Value == 123);
        a.Assert(result.BInteger24 == 0x10000);
    }

    [UnitTest]
    public static void Test_SpanIntBasicFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[] { 0, 0, 2, 0 };
        Span<byte> result = new byte[8];
        int leData = 0x20000;
        int beData = 0x200;
        int defaultData = Bytes.IsDefaultLE ? leData : beData;

        a.Assert(Bytes.From(defaultData, result) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            (object)defaultData,
            result             ) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.TryFrom(defaultData, result) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.TryFrom(
            (object)defaultData,
            result             ) == expectedResult.Length);

        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            leData                ,
            result                ,
            new(Endianness.Little)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            beData             ,
            result             ,
            new(Endianness.Big)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            defaultData            ,
            result                 ,
            new(Endianness.Default)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_SpanStringBasicFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = "Hello\0World"u8;
        Span<byte> result = new byte[expectedResult.Length * 3];
        string data = "Hello\0World";

        a.Assert(Bytes.From(data, result) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        expectedResult = Encoding.Unicode.GetBytes("Hello\0World");

        a.Assert(Bytes.From(
            data                   ,
            result                 ,
            new(Encoding: "utf-16")) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        expectedResult = "Hello\0World\0"u8;

        a.Assert(Bytes.From(
            data                     ,
            result                   ,
            new(NullTerminated: true)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_SpanIBinaryObjectFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[] { 0, 0, 2, 0 };
        Span<byte> result = new byte[8];
        int leData = 0x20000;
        int beData = 0x200;
        int defaultData = Bytes.IsDefaultLE ? leData : beData;

        a.Assert(Bytes.From(defaultData, result) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            leData                            ,
            result                            ,
            new(Endianness: Endianness.Little)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            beData                         ,
            result                         ,
            new(Endianness: Endianness.Big)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));

        a.Assert(Bytes.From(
            defaultData                        ,
            result                             ,
            new(Endianness: Endianness.Default)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_SpanAttributesFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[]
        {
            0, 0, 2, 0, // Property: 0x20000
            0, 2, // field: 2
            0x48, 0x65, 0x6c, 0x6c, 0x6f, 0, // Utf8String: Hello
            // Utf16String: World
            0x00, 0x57, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0, 0,
            255, 255, 255, 255, // SimpleBoa.Value: -1
            123, 0, 0, 0, // SimpleIBO.Value: 123
            0, 0, 1, 0, // BInteger24: 0x10000
        };
        Span<byte> result = new byte[40];
        BOAClass data = new()
        {
            Property = 0x20000,
            field = 2,
            Utf8String = "Hello",
            Utf16String = "World",
            SimpleBOA = new() { Value = -1, },
            SimpleIBO = new() { Value = 123 },
            BInteger24 = 0x10000,
        };

        a.Assert(Bytes.From(
            data                              ,
            result                            ,
            new(Endianness: Endianness.Little)) == expectedResult.Length);
        a.Assert(result.StartsWith(expectedResult));
    }

    [UnitTest]
    public static void Test_StreamIntBasicFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[] { 0, 0, 2, 0 };
        int leData = 0x20000;
        int beData = 0x200;
        int defaultData = Bytes.IsDefaultLE ? leData : beData;

        using (MemoryStream output = new())
        {
            Bytes.From(defaultData, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From((object)defaultData, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            a.Assert(Bytes.TryFrom(defaultData, output));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            a.Assert(Bytes.TryFrom((object)defaultData, output));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(leData, output, new(Endianness.Little));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(beData, output, new(Endianness.Big));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(defaultData, output, new(Endianness.Default));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }
    }

    [UnitTest]
    public static void Test_StreamStringBasicFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = "Hello\0World"u8;
        string data = "Hello\0World";

        using (MemoryStream output = new())
        {
            Bytes.From(data, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        expectedResult = Encoding.Unicode.GetBytes("Hello\0World");

        using (MemoryStream output = new())
        {
            Bytes.From(data, output, new(Encoding: "utf-16"));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        expectedResult = "Hello\0World\0"u8;

        using (MemoryStream output = new())
        {
            Bytes.From(data, output, new(NullTerminated: true));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }
    }

    [UnitTest]
    public static void Test_StreamIBinaryObjectFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[] { 0, 0, 2, 0 };
        int leData = 0x20000;
        int beData = 0x200;
        int defaultData = Bytes.IsDefaultLE ? leData : beData;

        using (MemoryStream output = new())
        {
            Bytes.From(defaultData, output);
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(leData, output, new(Endianness: Endianness.Little));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(beData, output, new(Endianness: Endianness.Big));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }

        using (MemoryStream output = new())
        {
            Bytes.From(
                defaultData                        ,
                output                             ,
                new(Endianness: Endianness.Default));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }
    }

    [UnitTest]
    public static void Test_StreamAttributesFrom(Asserter a)
    {
        ReadOnlySpan<byte> expectedResult = new byte[]
        {
            0, 0, 2, 0, // Property: 0x20000
            0, 2, // field: 2
            0x48, 0x65, 0x6c, 0x6c, 0x6f, 0, // Utf8String: Hello
            // Utf16String: World
            0x00, 0x57, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0, 0,
            255, 255, 255, 255, // SimpleBoa.Value: -1
            123, 0, 0, 0, // SimpleIBO.Value: 123
            0, 0, 1, 0, // BInteger24: 0x10000
        };
        BOAClass data = new()
        {
            Property = 0x20000,
            field = 2,
            Utf8String = "Hello",
            Utf16String = "World",
            SimpleBOA = new() { Value = -1, },
            SimpleIBO = new() { Value = 123 },
            BInteger24 = 0x10000,
        };

        using (MemoryStream output = new())
        {
            Bytes.From(data, output, new(Endianness: Endianness.Little));
            ReadOnlySpan<byte> result = output.ToArray();
            a.Assert(result.Length == expectedResult.Length);
            a.Assert(result.StartsWith(expectedResult));
        }
    }
}
