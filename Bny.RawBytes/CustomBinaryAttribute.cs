using System.Runtime.CompilerServices;

namespace Bny.RawBytes;

/// <summary>
/// Represents data that will be readed manually
/// </summary>
[AttributeUsage(    AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = true                                              ,
    Inherited     = false                                             )]
public class CustomBinaryAttribute : BinaryAttribute
{
    /// <summary>
    /// Size of the data to read
    /// </summary>
    public int Size => _size;
    readonly int _size;

    /// <summary>
    /// ID of the conversion (will be passed to the read/write method)
    /// </summary>
    public string? ID => _id;
    readonly string? _id;

    /// <summary>
    /// Represents data that will be converted in a custom way. The class in
    /// which is member with this attribute is must implement the
    /// IBinaryCustom interface
    /// </summary>
    /// <param name="size">Size of the data to read</param>
    /// <param name="id">
    /// ID of the conversion (will be passed to the read/write method)
    /// </param>
    /// <param name="order">
    /// Order of the attribute (line number by default)
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown for negative size
    /// </exception>
    public CustomBinaryAttribute(int size, string? id = null, [CallerLineNumber] int order = 0) : base(order)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        _size = size;
        _id = id;
    }
}
