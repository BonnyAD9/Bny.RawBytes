namespace Bny.RawBytes;

/// <summary>
/// Reads bytes as hex string
/// </summary>
public class HexBinaryEncoding : BinaryEncoding
{
    /// <inheritdoc/>
    public override string Name => "hex";

    private readonly byte[] nullTerm = new byte[1];

    /// <inheritdoc/>
    protected override byte[] GetNullTerminator() => nullTerm;

    /// <summary>
    /// The default constructor
    /// </summary>
    public HexBinaryEncoding()
    {
        Init();
    }

    /// <inheritdoc/>
    public override byte[] GetBytes(string str) => Convert.FromHexString(str);

    /// <inheritdoc/>
    public override string GetString(ReadOnlySpan<byte> data)
        => Convert.ToHexString(data);
}
