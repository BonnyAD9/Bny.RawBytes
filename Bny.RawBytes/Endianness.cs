namespace Bny.RawBytes;

/// <summary>
/// Byte order
/// </summary>
public enum Endianness
{
    /// <summary>
    /// The C# default (usually same little-endian)
    /// </summary>
    Default,
    /// <summary>
    /// Little-Endian byte order (least significant byte first)
    /// </summary>
    Little,
    /// <summary>
    /// Big-Endian byte order (most significant byte first)
    /// </summary>
    Big
}
