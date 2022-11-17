using Bny.RawBytes;

var arr = new byte[8];
var s = new MemoryStream(arr);

Bytes.From<BinaryTest>(new() { Width = 512, Height = 131072 }, s, Endianness.Big);
foreach (var i in arr)
    Console.WriteLine(i);

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

    // members without the attribute are ignored
    public int Size => Width * Height;

    public override string ToString() => $"[{Size}({Width} * {Height})]";
}