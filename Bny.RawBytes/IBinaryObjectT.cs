using System.Diagnostics.CodeAnalysis;

namespace Bny.RawBytes;

/// <summary>
/// Generic methods for converting the type to and from binary data
/// </summary>
public interface IBinaryObject<TSelf> : IBinaryObjectWrite where TSelf : IBinaryObject<TSelf>
{
    /// <summary>
    /// Tries to read the object from binary data
    /// </summary>
    /// <param name="data">binary data to read from</param>
    /// <param name="result">the resulting object, shouldn't be null of this returns true</param>
    /// <param name="readedBytes">Number of readed bytes</param>
    /// <param name="endianness">prefered endianness of the conversion</param>
    /// <returns>True on success, otherwise false</returns>
    public static abstract bool TryReadFromBinary(ReadOnlySpan<byte> data, [NotNullWhen(true)] out TSelf? result, out int readedBytes, Endianness endianness = Endianness.Default);
}
