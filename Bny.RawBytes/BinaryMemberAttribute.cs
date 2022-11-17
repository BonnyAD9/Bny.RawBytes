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
    private Endianness _endianness;

    /// <summary>
    /// Order of the attributes
    /// </summary>
    public int Order => _order;
    private int _order;

    /// <summary>
    /// Specifies whether when reading the value should be signed, works only for IBinaryNumber
    /// </summary>
    public Sign Signed => _signed;
    private Sign _signed;

    /// <summary>
    /// Creates new BinaryFieldAttribute
    /// </summary>
    /// <param name="endianness">prefered byte order of the conversion</param>
    /// <param name="signed">specifies whether when reading the value should be signed, works only for IBinaryNumber</param>
    /// <param name="order">order of the fields, this is by default the line number</param>
    public BinaryMemberAttribute(Endianness endianness = Endianness.Default, Sign signed = Sign.Default, [CallerLineNumber] int order = 0)
    {
        _endianness = endianness;
        _signed = signed;
        _order = order;
    }
}
