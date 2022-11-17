using System.Runtime.CompilerServices;

namespace Bny.RawBytes;

/// <summary>
/// Represents a binary field in a BinaryObject
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class BinaryMemberAttribute : Attribute
{
    /// <summary>
    /// Prefered byte order of the conversion
    /// </summary>
    public Endianness Endianness => _endianness;
    private readonly Endianness _endianness;

    /// <summary>
    /// Order of the attributes
    /// </summary>
    public int Order => _order;
    private readonly int _order;

    /// <summary>
    /// Specifies whether when reading the value should be signed, works only for IBinaryNumber
    /// </summary>
    public Sign Signed => _signed;
    private readonly Sign _signed;

    /// <summary>
    /// Specifies the size when reading, negative for default
    /// </summary>
    public int Size => _size;
    private readonly int _size;

    /// <summary>
    /// The string encoding, null for default encoding
    /// </summary>
    public string? Encoding => _encoding;
    private readonly string? _encoding;

    /// <summary>
    /// Determines whether the strings are null terminated
    /// </summary>
    public bool NullTerminated => _nullTerminated;
    private readonly bool _nullTerminated;

    /// <summary>
    /// Creates new BinaryFieldAttribute
    /// </summary>
    /// <param name="endianness">prefered byte order of the conversion</param>
    /// <param name="size">Specifies the size when reading, negative for default</param>
    /// <param name="signed">specifies whether when reading the value should be signed, works only for IBinaryNumber</param>
    /// <param name="encoding">The string encoding, null for default encoding</param>
    /// <param name="nullTerminated">Determines whether the string is null terminated</param>
    /// <param name="order">order of the fields, this is by default the line number</param>
    public BinaryMemberAttribute(Endianness endianness = Endianness.Default, int size = -1, Sign signed = Sign.Default, string? encoding = null, bool nullTerminated = false, [CallerLineNumber] int order = 0)
    {
        _endianness = endianness;
        _size= size;
        _signed = signed;
        _encoding = encoding;
        _nullTerminated = nullTerminated;
        _order = order;
    }
}
