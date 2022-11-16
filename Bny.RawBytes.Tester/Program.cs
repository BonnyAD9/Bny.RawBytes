using Bny.RawBytes;

var arr = new byte[] { 0, 2, 0, 0, 0, 2, 0, 0 };

// convert bytes (arr) to BinaryTest, the default byte order will be big-endian
Console.WriteLine(Bytes.To<BinaryTest>(arr, Endianness.Big));

[BinaryObject] // mark this as binary object
class BinaryTest
{
    // properties and fields with the BinaryMember attribute
    // will be readed in the same order in which they are written in code
    // or you can specify your own order with the Order property

    // Width will be readed first and always in little endian byte order
    [BinaryMember(endianness: Endianness.Little)]
    public int Width { get; init; }

    // Second will be readed height in the default byte order (big-endian in this case)
    [BinaryMember]
    public int Height { get; init; }

    public int Size => Width * Height;

    public override string ToString() => $"[{Size}({Width} * {Height})]";
}