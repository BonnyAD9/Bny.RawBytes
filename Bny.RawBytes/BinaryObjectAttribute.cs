namespace Bny.RawBytes;

/// <summary>
/// Represents a object that can be converted to and from binary data
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class BinaryObjectAttribute : Attribute
{
    /// <summary>
    /// byte order for fields with default byte order
    /// </summary>
    public Endianness Endianness => _endianness;
    private readonly Endianness _endianness;

    /// <summary>
    /// Creates a Binary object attribute
    /// </summary>
    /// <param name="endianness">byte order of fields with default byte order</param>
    public BinaryObjectAttribute(Endianness endianness = Endianness.Default)
    {
        _endianness = endianness;
    }
}
