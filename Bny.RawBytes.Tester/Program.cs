using Bny.RawBytes;
using System.Numerics;

var arr = new byte[] { 255, 255, 255, 255 }.AsSpan();
Console.WriteLine(Bytes.To<int>(arr, Endianness.Big)); // -1

arr = new byte[] { 2, 0, 255 };
Console.WriteLine(Bytes.To<ushort>(arr[..2], Endianness.Big)); // 512