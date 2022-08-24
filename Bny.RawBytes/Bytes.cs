using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bny.RawBytes;

/// <summary>
/// Contains functions for converting from and to byte array
/// </summary>
public static class Bytes
{
    /// <summary>
    /// Gets the default endianness.
    /// Returns <c>true</c> if the <c>Endianness.Default</c> conversion has same results as <c>Endianness.Little</c>, otherwise returns <c>false</c>.
    /// </summary>
    public static unsafe bool IsDefaultLE
    {
        get
        {
            short s = 1;
            return *(byte*)&s == 1;
        }
    }

    /// <summary>
    /// Reinterprets <c>byte</c> as <c>sbyte</c>
    /// </summary>
    /// <param name="b"><c>byte</c> to reinterpret</param>
    /// <returns><paramref name="b"/> interpreted as <code>sbyte</code></returns>
    public static sbyte ToSbyte(byte b) => unchecked((sbyte)b);

    /// <summary>
    /// Reinterprets <c>sbyte</c> as <c>byte</c>
    /// </summary>
    /// <param name="b"><c>sbyte</c> to reinterpret</param>
    /// <returns><paramref name="b"/> interpreted as <c>byte</c></returns>
    public static byte FromSbyte(sbyte b) => unchecked((byte)b);

    /// <summary>
    /// Converts <c>byte</c> sequence to <c>short</c> in <c>Endianness.Default</c> byte order
    /// </summary>
    /// <param name="bytes"><c>byte</c> sequence to convert. <c>bytes.Length</c> must be <c>&gt; 2</c>.</param>
    /// <returns><paramref name="bytes"/> converted to <c>short</c></returns>
    /// <exception cref="ArgumentException">thrown when <c>bytes.Length &lt; 2</c></exception>
    public static unsafe short ToShort(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(bytes));

        fixed (byte* ptr = bytes)
            return *(short*)&ptr;
    }

    /// <summary>
    /// Converts <c>byte</c> sequence to <c>short</c> in <c>Endianness.Little</c> byte order
    /// </summary>
    /// <param name="bytes"><c>byte</c> sequence to convert. <c>bytes.Length</c> must be <c>&gt; 2</c>.</param>
    /// <returns><paramref name="bytes"/> converted to <c>short</c></returns>
    /// <exception cref="ArgumentException">thrown when <c>bytes.Length &lt; 2</c></exception>
    public static short ToShortLE(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(bytes));

        return unchecked((short)(bytes[0] | bytes[1] << 8));
    }

    /// <summary>
    /// Converts <c>byte</c> sequence to <c>short</c> in <c>Endianness.Big</c> byte order
    /// </summary>
    /// <param name="bytes"><c>byte</c> sequence to convert. <c>bytes.Length</c> must be <c>&gt; 2</c>.</param>
    /// <returns><paramref name="bytes"/> converted to <c>short</c></returns>
    /// <exception cref="ArgumentException">thrown when <c>bytes.Length &lt; 2</c></exception>
    public static short ToShortBE(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(bytes));

        return unchecked((short)(bytes[0] << 8 | bytes[1]));
    }

    /// <summary>
    /// Converts <c>byte</c> sequence to <c>short</c> in the given endianness
    /// </summary>
    /// <param name="bytes"><c>byte</c> sequence to convert. <c>bytes.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="endianness">Endianness of the conversion</param>
    /// <returns><paramref name="bytes"/> converted to <c>short</c></returns>
    /// <exception cref="ArgumentException">Thrown for invalid <paramref name="endianness"/> values and when <c>bytes.Length &lt; 2</c></exception>
    public static short ToShort(ReadOnlySpan<byte> bytes, Endianness endianness) => endianness switch
    {
        Endianness.Default => ToShort(bytes),
        Endianness.Little => ToShortLE(bytes),
        Endianness.Big => ToShortBE(bytes),
        _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
    };

    /// <summary>
    /// Converts <c>short</c> to sequence of bytes in <c>Endianness.Default</c> byte order
    /// </summary>
    /// <param name="output">Where to copy the bytes. <c>output.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="value"><c>short</c> to convert to <c>byte</c> sequence</param>
    /// <exception cref="ArgumentException">Thrown when <c>output.Length &lt; 2</c></exception>
    public static unsafe void FromShort(Span<byte> output, short value) => new ReadOnlySpan<byte>((byte*)&value, 2).CopyTo(output);

    /// <summary>
    /// Converts <c>short</c> to sequence of bytes in <c>Endianness.Little</c> byte order
    /// </summary>
    /// <param name="output">Where to copy the bytes. <c>output.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="value"><c>short</c> to convert to <c>byte</c> sequence</param>
    /// <exception cref="ArgumentException">Thrown when <c>output.Length &lt; 2</c></exception>
    public static void FromShortLE(Span<byte> output, short value)
    {
        if (output.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(output));

        unchecked
        {
            output[0] = (byte)value;
            output[1] = (byte)(value >> 8);
        }
    }

    /// <summary>
    /// Converts <c>short</c> to sequence of bytes in <c>Endianness.Big</c> byte order
    /// </summary>
    /// <param name="output">Where to copy the bytes. <c>output.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="value"><c>short</c> to convert to <c>byte</c> sequence</param>
    /// <exception cref="ArgumentException">Thrown when <c>output.Length &lt; 2</c></exception>
    public static void FromShortBE(Span<byte> output, short value)
    {
        if (output.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(output));

        unchecked
        {
            output[0] = (byte)(value >> 8);
            output[1] = (byte)value;
        }
    }

    /// <summary>
    /// Converts <c>short</c> to sequence of bytes in the specified byte order.
    /// </summary>
    /// <param name="output">Where to copy the bytes. <c>output.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="value"><c>short</c> to convert to <c>byte</c> sequence</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endianness"/> has invalid value or when <c>output.Length &lt; 2</c></exception>
    public static void FromShort(Span<byte> output, short value, Endianness endianness)
    {
        switch (endianness)
        {
            case Endianness.Default:
                FromShort(output, value);
                return;
            case Endianness.Little:
                FromShortLE(output, value);
                return;
            case Endianness.Big:
                FromShortBE(output, value);
                return;
        }

        throw new ArgumentException("Invalid endianness value", nameof(endianness));
    }
}
