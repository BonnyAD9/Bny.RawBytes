namespace Bny.RawBytes;

/// <summary>
/// Parameters for the Bytes methods
/// </summary>
/// <param name="Endianness">Byte order of the operation</param>
/// <param name="Sign">The prefered sign for IBinaryNumbers</param>
/// <param name="Encoding">The default encoding for strings</param>
public record BytesParam(Endianness Endianness = Endianness.Default, Sign Sign = Sign.Default, string Encoding = "utf-8")
{
    // to silence the warning
    private Type? _type;
    internal Type Type
    {
        get => _type!;
        set => _type = value;
    }

    internal bool IsSigned(bool def) => Sign == Sign.Default ? def : Sign == Sign.Signed;
    internal Endianness GetEndianness(Endianness def) => Endianness == Endianness.Default ? def : Endianness;
    internal Endianness GetEndianness() => GetEndianness(Bytes.DefaultEndianness);
    internal string? GetString(ReadOnlySpan<byte> data) => BinaryEncoding.TryGet(Encoding)?.GetString(data);
    internal byte[]? GetBytes(string str) => BinaryEncoding.TryGet(Encoding)?.GetBytes(str);

    internal object? CreateInstance()
    {
        try
        {
            return Activator.CreateInstance(Type);
        }
        catch
        {
            return null;
        }
    }
}
