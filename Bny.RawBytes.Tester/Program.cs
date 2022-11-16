using Bny.RawBytes;

var arr = new byte[] { 255, 255, 255, 255, 0, 2, 0, 0 };
var s = new MemoryStream(arr);
Console.WriteLine(Bytes.To<BinaryTest>(s));

s.Position = 0;
Bytes.From(new BinaryTest(512, -1), s);
foreach (var i in arr)
    Console.WriteLine(i);

record BinaryTest(int Width, int Height) : IBinaryObject<BinaryTest>
{
    public static int ReadSize => 8;
    public int WriteSize => ReadSize;

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
}