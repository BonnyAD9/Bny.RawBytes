namespace Bny.RawBytes;

/// <summary>
/// Specifies whether a integer is signed or unsigned
/// </summary>
public enum Sign
{
    /// <summary>
    /// Signed if the type supports it, otherwise unsigned
    /// </summary>
    Default,
    /// <summary>
    /// Unsigned
    /// </summary>
    Unsigned,
    /// <summary>
    /// Signed
    /// </summary>
    Signed,
}
