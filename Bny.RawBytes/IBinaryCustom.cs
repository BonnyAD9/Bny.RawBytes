namespace Bny.RawBytes;

/// <summary>
/// Provides advanced ways of reading using attributes
/// </summary>
public interface IBinaryCustom
{
    /// <summary>
    /// Reads the given binary data in a custom way
    /// </summary>
    /// <param name="data">Data to read</param>
    /// <param name="id">ID of the read call (from attribute)</param>
    /// <param name="param">the conversion parameters</param>
    /// <returns>True on success, otherwise false</returns>
    public bool TryReadCustom(
        ReadOnlySpan<byte> data ,
        string?            id   ,
        BytesParam         param);

    /// <summary>
    /// Writes the binary data in a custom way
    /// </summary>
    /// <param name="output">Where to write the data</param>
    /// <param name="id">ID of the write call (from attribute)</param>
    /// <param name="param">the conversion parameters</param>
    /// <returns>True on success, otherwise false</returns>
    public bool TryWriteCustom(
        Span<byte> output,
        string?    id    ,
        BytesParam param );
}
