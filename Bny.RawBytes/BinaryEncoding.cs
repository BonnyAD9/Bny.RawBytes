using System.Runtime.CompilerServices;
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
}
