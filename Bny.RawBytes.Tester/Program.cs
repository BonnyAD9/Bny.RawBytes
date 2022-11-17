using Bny.RawBytes;
using System.Text;

foreach (var e in Encoding.GetEncodings())
    Console.WriteLine($"{e.DisplayName} --- {e.Name}");