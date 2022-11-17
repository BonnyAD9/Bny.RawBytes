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

/// <summary>
/// Extension class for the Sign enum
/// </summary>
public static class SignExtensions
{
    /// <summary>
    /// Gets the value of sign
    /// </summary>
    /// <param name="sign">the sign</param>
    /// <param name="default">value to use if sign has value Default</param>
    /// <returns>@default if sign has default value otherwise sign</returns>
    public static bool IsSigned(this Sign sign, bool @default) => sign == Sign.Default ? @default : sign == Sign.Signed;
}
