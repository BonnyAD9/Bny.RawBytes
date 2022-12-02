using System.Numerics;
using System.Text;

namespace Bny.RawBytes.Tests;

[UnitTest]
internal class BytesTests
{
    [UnitTest]
    public static void Test_Defaults(Asserter a)
    {
        a.Assert(Bytes.IsDefaultLE == BitConverter.IsLittleEndian);
        
        Endianness endianness = BitConverter.IsLittleEndian ? Endianness.Little : Endianness.Big;

        a.Assert(Bytes.DefaultEndianness == endianness);
    }

    [UnitTest]
    public static void Test_IntBasicTo(Asserter a)
    {
        ReadOnlySpan<byte> data = new byte[] { 0, 0, 2, 0, 255, 255, 255, 255 };
        int leResult = 0x20000;
        int beResult = 0x200;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<int>(data) == defaultResult);
        a.Assert(Bytes.To<int>(data, out int readedBytes) == defaultResult);
        a.Assert(readedBytes == sizeof(int));
        a.Assert((int)Bytes.To(data, typeof(int)) == defaultResult);
        a.Assert((int)Bytes.To(data, typeof(int), out readedBytes) == defaultResult);
        a.Assert(readedBytes == sizeof(int));

        a.Assert(Bytes.TryTo(data, out int result) == sizeof(int));
        a.Assert(result == defaultResult);
        a.Assert(Bytes.TryTo(data, typeof(int), out object? objResult) == sizeof(int));
        a.Assert((int)objResult! == defaultResult);

        a.Assert(Bytes.To<int>(data, new(Endianness.Little)) == leResult);
        a.Assert(Bytes.To<int>(data, new(Endianness.Big)) == beResult);
        a.Assert(Bytes.To<int>(data, new(Endianness.Default)) == defaultResult);
    }

    [UnitTest]
    public static void Test_StringBasicTo(Asserter a)
    {
        a.Assert(Bytes.To<string>("Hello\0World"u8) == "Hello\0World");
        a.Assert(Bytes.To<string>("Hello\0World"u8, out int readedBytes, new(NullTerminated: true)) == "Hello");
        a.Assert(readedBytes == "Hello\0".Length);

        var utf16Bytes = Encoding.Unicode.GetBytes("Hello\0World");

        a.Assert(Bytes.To<string>(utf16Bytes, new(Encoding: "utf-16")) == "Hello\0World");
        a.Assert(Bytes.To<string>(utf16Bytes, out readedBytes, new(Encoding: "utf-16", NullTerminated: true)) == "Hello");
        a.Assert(readedBytes == "Hello\0".Length * 2);
    }

    [UnitTest]
    public static void Test_IBinaryIntegerTo(Asserter a)
    {
        ReadOnlySpan<byte> data = new byte[] { 0, 0, 2, 0, 0, 0};
        int beResult = 0x2000000;
        int leResult = 0x20000;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<BigInteger>(data) == defaultResult);
        a.Assert(Bytes.To<BigInteger>(data, new(Endianness: Endianness.Little)) == leResult);
        a.Assert(Bytes.To<BigInteger>(data, new(Endianness: Endianness.Big)) == beResult);
        a.Assert(Bytes.To<BigInteger>(data, new(Endianness: Endianness.Default)) == defaultResult);

        data = new byte[] { 255, 255, 255, 255 };

        a.Assert(Bytes.To<BigInteger>(data) == -1);
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Default)) == -1);
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Signed)) == -1);
        a.Assert(Bytes.To<BigInteger>(data, new(Sign: Sign.Unsigned)) == uint.MaxValue);
    }

    class SimpleIBOClass : IBinaryObject<SimpleIBOClass>
    {
        public static int ReadSize => 4;

        public int Value { get; set; }

        public int WriteSize => throw new NotImplementedException();

        public static int TryReadFromBinary(ReadOnlySpan<byte> data, out SimpleIBOClass? result, Endianness endianness = Endianness.Default)
        {
            result = new() { Value = Bytes.To<int>(data, out int readedBytes, new(Endianness: endianness)) };
            return readedBytes;
        }

        public int TryWriteToBinary(Span<byte> data, Endianness endianness) => Bytes.From(Value, data, new(Endianness: endianness));
    }

    [UnitTest]
    public static void Test_IBinaryObjectTo(Asserter a)
    {
        ReadOnlySpan<byte> data = new byte[] { 0, 0, 2, 0, 255, 255, 255, 255 };
        int leResult = 0x20000;
        int beResult = 0x200;
        int defaultResult = Bytes.IsDefaultLE ? leResult : beResult;

        a.Assert(Bytes.To<SimpleIBOClass>(data).Value == defaultResult);
        a.Assert(Bytes.To<SimpleIBOClass>(data, new(Endianness: Endianness.Default)).Value == defaultResult);
        a.Assert(Bytes.To<SimpleIBOClass>(data, new(Endianness: Endianness.Little)).Value == leResult);
        a.Assert(Bytes.To<SimpleIBOClass>(data, new(Endianness: Endianness.Big)).Value == beResult);
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

        [BinaryMember(size: 3)]
        public BigInteger BInteger24 { get; set; }
    }

    [UnitTest]
    public static void Test_AttributesTo(Asserter a)
    {
        var data = new byte[]
        {
            0, 0, 2, 0, // Property: 0x20000
            0, 2, // field: 2
            0x48, 0x65, 0x6c, 0x6c, 0x6f, 0, // Utf8String: Hello
            0x00, 0x57, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0, 0, // Utf16String: World
            255, 255, 255, 255, // SimpleBoa.Value: -1
            123, 0, 0, 0, // SimpleIBO.Value: 123
            0, 1, 0, // BInteger24: 0x100
            255, 255, 255, 255, 255, // Some additional data
        };

        var result = Bytes.To<BOAClass>(data, out int readedBytes, new(Endianness: Endianness.Little));

        a.Assert(readedBytes == data.Length - 5);
        a.Assert(result.Property == 0x20000);
        a.Assert(result.field == 2);
        a.Assert(result.Utf8String == "Hello");
        a.Assert(result.Utf16String == "World");
        a.Assert(result.SimpleBOA is not null);
        a.Assert(result.SimpleBOA!.Value == -1);
        a.Assert(result.SimpleIBO is not null);
        a.Assert(result.SimpleIBO!.Value == 123);
        a.Assert(result.BInteger24 == 0x100);
    }
}
