# Bny.RawBytes
C# library for converting from and to bytes. Without the boring repetition.

## In this repository
- **Bny.RawBytes:** the library
- **Bny.RawBytes.Tester:** unit tests

## How to use

### Code

```csharp
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
    
    // members without the attribute are ignored
    public int Size => Width * Height;

    public override string ToString() => $"[{Size}({Width} * {Height})]";
}
```

### Output

```
[67108864(512 * 131072)]
```

## Links
- **Author:** [BonnyAD9](https://github.com/BonnyAD9)
- **GitHub repository:** [Bny.RawBytes](https://github.com/BonnyAD9/Bny.RawBytes)
- **Documentation:** [Doxygen](https://bonnyad9.github.io/Bny.RawBytes/)
- **My Website:** [bonnyad9.github.io](https://bonnyad9.github.io/)
