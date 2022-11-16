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
    /// Creates new BinaryFieldAttribute
    /// </summary>
    /// <param name="endianness">prefered byte order of the conversion</param>
    /// <param name="order">order of the fields</param>
    public BinaryMemberAttribute(Endianness endianness = Endianness.Default, [CallerLineNumber] int order = 0)
    {
        _endianness = endianness;
        _order = order;
    }
}
