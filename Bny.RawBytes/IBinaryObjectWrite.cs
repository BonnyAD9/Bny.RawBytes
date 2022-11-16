namespace Bny.RawBytes;

/// <summary>
/// Represents a object that can be written into binary
/// </summary>
public interface IBinaryObjectWrite
{
    /// <summary>
    /// Tries to write the object to binary
    /// </summary>
    /// <param name="data">Where to write</param>
    /// <param name="endianness">Preferred endianness or default</param>
    /// <returns>number of written bytes on success, otherwise negative</returns>
    public int TryWriteToBinary(Span<byte> data, Endianness endianness);

    /// <summary>
    /// The number of bytes that would be written by TryWriteToBinary
    /// </summary>
    public int WriteSize { get; }
}
