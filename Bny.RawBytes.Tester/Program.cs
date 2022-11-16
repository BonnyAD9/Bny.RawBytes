using Bny.RawBytes;

var arr = new byte[] { 1, 0, 0, 0 };

Console.WriteLine(Bytes.To<int>(arr));