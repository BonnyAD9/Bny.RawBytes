using System.Diagnostics.CodeAnalysis;

namespace Bny.RawBytes;

/// <summary>
/// Methods for converting the type to and from binary data
/// </summary>
public interface IBinaryObject
{
    /// <summary>
    /// Tries to read the object from binary data
    /// </summary>
    /// <param name="data">binary data to read from</param>
    /// <param name="result">the resulting object, shouldn't be null of this returns true</param>
    /// <param name="endianness">prefered endianness of the conversion</param>
    /// <returns>True on success, otherwise false</returns>
    public static abstract bool TryFromBinary(ReadOnlySpan<byte> data, [NotNullWhen(true)] out object? result, Endianness endianness = Endianness.Default);

    /// <summary>
    /// Tries to convert the object to binary data
    /// </summary>
    /// <param name="result">result binary data</param>
    /// <param name="endianness">prefered endianness of the conversion or default</param>
    /// <returns>Number of bytes written to result, negative on error</returns>
    public int TryToBinary(Span<byte> result, Endianness endianness = Endianness.Default);
}
