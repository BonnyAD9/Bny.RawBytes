using System.Diagnostics.CodeAnalysis;

namespace Bny.RawBytes;

/// <summary>
/// Generic methods for converting the type to and from binary data
/// </summary>
public interface IBinaryObject<TSelf> : IBinaryObjectStream<TSelf>
    where TSelf : IBinaryObject<TSelf>
{
    /// <summary>
    /// Number of bytes that will be readed when calling TryReadFromBinary
    /// </summary>
    static abstract int ReadSize { get; }

    static bool IBinaryObjectStream<TSelf>.TryReadFromBinary(
                                Stream     data      ,
        [NotNullWhen(true)] out TSelf?     result    ,
                                Endianness endianness)
    {
        result = default;
        Span<byte> buffer = new byte[TSelf.ReadSize];
        if (data.Read(buffer) != buffer.Length)
            return false;
        return TSelf.TryReadFromBinary(buffer, out result, endianness) >= 0;
    }
}
