using Bny.RawBytes;

var arr = new byte[] { 255, 255, 255, 255, 0, 2, 0, 0 };
Console.WriteLine(Bytes.To<BinaryTest>(arr));

Bytes.From(new BinaryTest(512, -1), arr);
foreach (var i in arr)
    Console.WriteLine(i);

record BinaryTest(int Width, int Height) : IBinaryObject<BinaryTest>
{
    public int WriteSize => 8;

    public static int TryReadFromBinary(ReadOnlySpan<byte> data, out BinaryTest? result, Endianness endianness = Endianness.Default)
    {
        result = null;
        if (data.Length < 8)
            return -1;
        result = new(Bytes.To<int>(data[..4], endianness), Bytes.To<int>(data[4..], endianness));
        return 8;
    }

    public int TryWriteToBinary(Span<byte> data, Endianness endianness)
    {
        if (data.Length < 8)
            return -1;
        Bytes.From(Width, data, endianness);
        Bytes.From(Height, data[4..], endianness);
        return 8;
    }

    public BinaryTest() : this(0, 0) { }
}