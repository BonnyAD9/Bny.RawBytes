using Bny.RawBytes;

byte[] arr = new byte[] { 255, 255, 255, 255 };
int val = Bytes.To<int>(arr, Endianness.Big);

Console.WriteLine(val); // -1