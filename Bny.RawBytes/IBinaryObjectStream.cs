using System.Diagnostics.CodeAnalysis;

namespace Bny.RawBytes;

/// <summary>
/// Generic methods for converting the type to and from binary data
/// </summary>
public interface IBinaryObjectStream<TSelf> : IBinaryObjectWrite where TSelf : IBinaryObjectStream<TSelf>
{
    /// <summary>
    /// Tries to read the object from binary data
    /// </summary>
    /// <param name="data">binary data to read from</param>
    /// <param name="result">the resulting object, shouldn't be null of this returns positive</param>
    /// <param name="endianness">prefered endianness of the conversion</param>
    /// <returns>number of written bytes on success, otherwise negative</returns>
    public static abstract int TryReadFromBinary(ReadOnlySpan<byte> data, out TSelf? result, Endianness endianness = Endianness.Default);

    /// <summary>
    /// Tries to read the object from binary stream
    /// </summary>
    /// <param name="data">binary stream to read from</param>
    /// <param name="result">the resulting object, shouldn't be null of this returns positive</param>
    /// <param name="endianness">prefered endianness of the conversion</param>
    /// <returns>number of written bytes on success, otherwise negative</returns>
    public static abstract bool TryReadFromBinary(Stream data, [NotNullWhen(true)] out TSelf? result, Endianness endianness = Endianness.Default);
}
