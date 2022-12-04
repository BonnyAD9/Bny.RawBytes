using System.Runtime.CompilerServices;

namespace Bny.RawBytes;

/// <summary>
/// Specifies padding in the binary representation
/// </summary>
[AttributeUsage(
    AttributeTargets.Field   |
    AttributeTargets.Property,
    AllowMultiple = true     ,
    Inherited     = false    )]
public class BinaryPaddingAttribute : BinaryAttribute
{
    /// <summary>
    /// Size of the padding
    /// </summary>
    public int Size => _size;
    private int _size;

    /// <summary>
    /// Initializes new padding attribute
    /// </summary>
    /// <param name="size">Size of the padding</param>
    /// <param name="order">
    /// Ordering of the attributes, this is by default line number
    /// </param>
    public BinaryPaddingAttribute(int size = 1, [CallerLineNumber] int order = 0) : base(order)
    {
        _size = size;
    }
}
