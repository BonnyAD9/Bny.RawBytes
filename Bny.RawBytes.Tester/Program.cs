using Bny.RawBytes;
using System.Text;

var arr = "czeThe Best Track\0garbage"u8;
Console.WriteLine(Bytes.To<BinaryTest>(arr));

[BinaryObject]
class BinaryTest
{
    [BinaryMember(size: 3, encoding: "us-ascii")]
    string Language { get; init; } = "";

    [BinaryMember(nullTerminated: true)]
    string Comment { get; init; } = "";

    public override string ToString() => $"{Language}: {Comment}";
}