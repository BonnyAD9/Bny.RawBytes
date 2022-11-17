using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Bny.RawBytes;

/// <summary>
/// Represents encoding used in binary conversions
/// </summary>
public abstract class BinaryEncoding
{
    /// <summary>
    /// Collection of all encodings
    /// </summary>
    public static Dictionary<string, BinaryEncoding> Encodings { get; } = new();

    static BinaryEncoding()
    {
        foreach (var e in Encoding.GetEncodings())
            Add(new NetBinaryEncoding(e));
    }

    /// <summary>
    /// The null terminator for the encoding
    /// </summary>
    protected byte[] _nullTerminator;

    /// <summary>
    /// The null terminator for the encoding
    /// </summary>
    public ReadOnlySpan<byte> NullTerminator => _nullTerminator;
    
    /// <summary>
    /// Basic inicialization, call the Init for full inicialization
    /// </summary>
    public BinaryEncoding()
    {
        _nullTerminator = null!; // silence the warning
    }

    /// <summary>
    /// Call this when your cobject is inicialized
    /// </summary>
    protected void Init()
    {
        _nullTerminator = GetNullTerminator();
    }

    /// <summary>
    /// Gets the null terminator of the encoding
    /// </summary>
    /// <returns></returns>
    protected virtual byte[] GetNullTerminator() => GetBytes("\0");

    /// <summary>
    /// Adds the given encoding to the encodings
    /// </summary>
    /// <param name="encoding"></param>
    public static void Add(BinaryEncoding encoding) => Encodings[encoding.Name] = encoding;

    /// <summary>
    /// Tries to get the encoding
    /// </summary>
    /// <param name="name">name of the encoding</param>
    /// <returns>The encoding or null</returns>
    public static BinaryEncoding? TryGet(string name) => Encodings.GetValueOrDefault(name);

    /// <summary>
    /// Unique name of this encoding
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Decodes the gicen bytes into a string
    /// </summary>
    /// <param name="data">bytes to decode</param>
    /// <returns>string</returns>
    public abstract string GetString(ReadOnlySpan<byte> data);

    /// <summary>
    /// Encodes the given string into a byte array
    /// </summary>
    /// <param name="str">string to encode</param>
    /// <returns>byte array</returns>
    public abstract byte[] GetBytes(string str);

    /// <summary>
    /// Decodes the binary data until the null character
    /// </summary>
    /// <param name="data">data to decode</param>
    /// <param name="readedBytes">number of decoded bytes</param>
    /// <returns>Decoded string</returns>
    public virtual string GetString(ReadOnlySpan<byte> data, out int readedBytes)
    {
        int ind = data.LenIndexOf(NullTerminator);
        if (ind >= 0)
            data = data[..ind];
        readedBytes = data.Length + _nullTerminator.Length;
        return GetString(data);
    }

    /// <summary>
    /// Reads all the bytes from the string and decodes them to a string
    /// </summary>
    /// <param name="s">Stream to read from</param>
    /// <returns>Decoded string</returns>
    public virtual string GetString(Stream s)
    {
        List<byte> data = new();
        int i;
        while ((i = s.ReadByte()) != -1)
            data.Add((byte)i);
        return GetString(CollectionsMarshal.AsSpan(data));
    }

    /// <summary>
    /// Reads data from the string until a null character and decodes them to a string
    /// </summary>
    /// <param name="s">Stream to read from</param>
    /// <param name="readedBytes">Number of bytes readed from the string</param>
    /// <returns>The decoded string</returns>
    public virtual string GetString(Stream s, out int readedBytes)
    {
        List<byte> data = new();
        var buffer = new byte[_nullTerminator.Length];
        while (s.Read(buffer) == _nullTerminator.Length && !NullTerminator.StartsWith(buffer))
            data.AddRange(buffer);
        readedBytes = data.Count;
        return GetString(CollectionsMarshal.AsSpan(data));
    }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
