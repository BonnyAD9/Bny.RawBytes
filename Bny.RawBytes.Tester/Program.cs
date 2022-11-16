using Bny.RawBytes;
using System.Diagnostics.CodeAnalysis;

var arr = new byte[] { 255, 255, 255, 255, 0, 2, 0, 0 };
Console.WriteLine(Bytes.To<BinaryTest>(arr));

Bytes.From(new BinaryTest(512, -1), arr);
foreach (var i in arr)
    Console.WriteLine(i);

record BinaryTest(int Width, int Height) : IBinaryObject<BinaryTest>
{
    public int WriteSize => 8;

    public static bool TryReadFromBinary(ReadOnlySpan<byte> data, [NotNullWhen(true)] out BinaryTest? result, out int readedBytes, Endianness endianness = Endianness.Default)
    {
        readedBytes = 0;
        result = null;
        if (data.Length < 8)
            return false;
        result = new(Bytes.To<int>(data[..4], endianness), Bytes.To<int>(data[4..], endianness));
        readedBytes = 8;
        return true;
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