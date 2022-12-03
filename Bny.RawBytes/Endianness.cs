namespace Bny.RawBytes;

/// <summary>
/// Byte order. <c>Endianness.Default</c> byte order is usually the fastest
/// because it usually is just noop or copy.
/// </summary>
public enum Endianness
{
    /// <summary>
    /// The C# default (usually same as <c>Endianess.Little</c>).
    /// You can get the default endianness with <c>Bytes.IsDefaultLE</c>
    /// property. This is usually faster than the other two.
    /// </summary>
    Default,
    /// <summary>
    /// Little-Endian byte order (least significant byte first)
    /// </summary>
    Little,
    /// <summary>
    /// Big-Endian byte order (most significant byte first)
    /// </summary>
    Big,
}
