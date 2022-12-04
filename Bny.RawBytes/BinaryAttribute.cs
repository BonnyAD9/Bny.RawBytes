namespace Bny.RawBytes;

/// <summary>
/// The base attribute for all the other binary attributes
/// Don't derive from this class (outside of the library)
/// </summary>
public abstract class BinaryAttribute : Attribute
{
    /// <summary>
    /// Ordering of the attributes
    /// </summary>
    public int Order => _order;
    private readonly int _order;

    /// <summary>
    /// Sets the order in this attribute
    /// </summary>
    /// <param name="order">Order of the attribute</param>
    public BinaryAttribute(int order)
    {
        _order = order;
    }
}
