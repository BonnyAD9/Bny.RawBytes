namespace Bny.RawBytes.Streams;

/// <summary>
/// Extensions for conversions with streams
/// </summary>
public static class StreamExtensinos
{
    /// <summary>
    /// Writes <c>sbyte to <paramref name="s"/></c>
    /// </summary>
    /// <param name="s">Where to write</param>
    /// <param name="value">What to write</param>
    /// <returns><paramref name="value"/> converted to <c>byte</c></returns>
    public static byte WriteInt8(this Stream s, sbyte value)
    {
        byte b = Bytes.FromInt8(value);
        s.WriteByte(b);
        return b;
    }

    /// <summary>
    /// Reads <c>sbyte</c> from <paramref name="s"/>
    /// </summary>
    /// <param name="s">Where to read from</param>
    /// <returns>Readed <c>sbyte</c></returns>
    /// <exception cref="IOException">Thrown when the <paramref name="s"/> returns EOF</exception>
    public static sbyte ReadInt8(this Stream s)
    {
        var b = s.ReadByte();

        if (b < 0)
            throw new IOException("End of stream");

        return Bytes.ToInt8((byte)b);
    }

    /// <summary>
    /// Tries to read <c>sbyte</c> from <paramref name="s"/>
    /// </summary>
    /// <param name="s">Where to read form</param>
    /// <param name="value">Where to writed the readed value</param>
    /// <returns>True if the reading was successfull, otherwise false</returns>
    public static bool TryReadInt8(this Stream s, out sbyte value)
    {
        value = default;
        int b;

        try
        {
            b = s.ReadByte();
        }
        catch
        {
            return false;
        }

        if (b < 0)
            return false;

        Bytes.ToInt8((byte)b);
        return true;
    }
}
