using Bny.RawBytes;
using System.Numerics;

var arr = new byte[4];

Bytes.From<ushort>(512, arr);

foreach (var b in arr)
    Console.WriteLine(b);