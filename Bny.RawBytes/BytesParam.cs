namespace Bny.RawBytes;

/// <summary>
/// Parameters for the Bytes methods
/// </summary>
/// <param name="Endianness">Byte order of the operation</param>
/// <param name="Sign">The prefered sign for IBinaryNumbers</param>
/// <param name="Encoding">The default encoding for strings</param>
/// <param name="NullTerminated">Determines whether the strings are null terminated</param>
public record BytesParam(Endianness Endianness = Endianness.Default, Sign Sign = Sign.Default, string Encoding = "utf-8", bool NullTerminated = false)
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
    internal Endianness GetEndiannessTo(Endianness other) => other == Endianness.Default ? Endianness : other;
    internal Endianness GetEndianness() => GetEndianness(Bytes.DefaultEndianness);
    internal BinaryEncoding? GetEncoding() => BinaryEncoding.TryGet(Encoding);

    internal string? GetString(ReadOnlySpan<byte> data, out int bytesReaded)
    {
        bytesReaded = 0;
        if (NullTerminated)
            return GetEncoding()?.GetString(data, out bytesReaded);
        bytesReaded = data.Length;
        return GetEncoding()?.GetString(data);
    }

    internal string? GetString(Stream stream) => NullTerminated
        ? GetEncoding()?.GetString(stream, out _)
        : GetEncoding()?.GetString(stream);


    internal int GetBytes(string str, Span<byte> result)
    {
        var enc = GetEncoding();
        if (enc is null)
            return -1;

        if (NullTerminated)
            str += '\0';

        ReadOnlySpan<byte> strb = enc.GetBytes(str);
        if (strb.Length > result.Length)
            return -1;

        strb.CopyTo(result);
        return strb.Length;
    }

    internal bool GetBytes(string str, Stream output)
    {
        var enc = GetEncoding();
        if (enc is null)
            return false;

        output.Write(enc.GetBytes(str));
        if (NullTerminated)
            output.Write(enc.NullTerminator);
        return true;
    }

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
