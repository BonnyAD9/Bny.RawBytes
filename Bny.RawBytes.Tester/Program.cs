using Bny.RawBytes;
using System.Text;

var arr = new byte[] { 0x61, 0x68, 0x6f, 0x6a };
Console.WriteLine(Bytes.To<string>(arr));