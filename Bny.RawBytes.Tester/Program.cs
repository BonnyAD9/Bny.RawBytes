﻿using Bny.RawBytes;

var arr = new byte[] { 255, 255, 255, 255, 0, 2, 0, 0 };

var res = Bytes.To<BinaryTest>(arr);

Console.WriteLine(res);

[BinaryObject]
class BinaryTest
{
    [BinaryMember]
    public int Width { get; init; }

    [BinaryMember]
    public int Height { get; init; }

    public override string ToString() => $"[{Width} {Height}]";
}