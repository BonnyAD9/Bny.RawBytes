﻿namespace Bny.RawBytes;

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
    public static sbyte ToInt8(byte b) => unchecked((sbyte)b);

    /// <summary>
    /// Reinterprets <c>sbyte</c> as <c>byte</c>
    /// </summary>
    /// <param name="b"><c>sbyte</c> to reinterpret</param>
    /// <returns><paramref name="b"/> interpreted as <c>byte</c></returns>
    public static byte FromInt8(sbyte b) => unchecked((byte)b);

    /// <summary>
    /// Converts <c>byte</c> sequence to <c>short</c> in <c>Endianness.Default</c> byte order
    /// </summary>
    /// <param name="bytes"><c>byte</c> sequence to convert. <c>bytes.Length</c> must be <c>&gt; 2</c>.</param>
    /// <returns><paramref name="bytes"/> converted to <c>short</c></returns>
    /// <exception cref="ArgumentException">thrown when <c>bytes.Length &lt; 2</c></exception>
    public static unsafe short ToInt16(ReadOnlySpan<byte> bytes)
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
    public static short ToInt16LE(ReadOnlySpan<byte> bytes)
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
    public static short ToInt16BE(ReadOnlySpan<byte> bytes)
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
    public static short ToInt16(ReadOnlySpan<byte> bytes, Endianness endianness) => endianness switch
    {
        Endianness.Default => ToInt16(bytes),
        Endianness.Little => ToInt16LE(bytes),
        Endianness.Big => ToInt16BE(bytes),
        _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
    };

    /// <summary>
    /// Converts <c>short</c> to sequence of bytes in <c>Endianness.Default</c> byte order
    /// </summary>
    /// <param name="output">Where to copy the bytes. <c>output.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="value"><c>short</c> to convert to <c>byte</c> sequence</param>
    /// <exception cref="ArgumentException">Thrown when <c>output.Length &lt; 2</c></exception>
    public static unsafe void FromInt16(Span<byte> output, short value)
    {
        if (output.Length < 2)
            throw new ArgumentException("The span length is smaller than 2", nameof(output));

        fixed (byte* ptr = output)
            *(short*)ptr = value;
    }

    /// <summary>
    /// Converts <c>short</c> to sequence of bytes in <c>Endianness.Little</c> byte order
    /// </summary>
    /// <param name="output">Where to copy the bytes. <c>output.Length</c> must be <c>&gt; 2</c>.</param>
    /// <param name="value"><c>short</c> to convert to <c>byte</c> sequence</param>
    /// <exception cref="ArgumentException">Thrown when <c>output.Length &lt; 2</c></exception>
    public static void FromInt16LE(Span<byte> output, short value)
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
    public static void FromInt16BE(Span<byte> output, short value)
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
    public static void FromInt16(Span<byte> output, short value, Endianness endianness)
    {
        switch (endianness)
        {
            case Endianness.Default:
                FromInt16(output, value);
                return;
            case Endianness.Little:
                FromInt16LE(output, value);
                return;
            case Endianness.Big:
                FromInt16BE(output, value);
                return;
        }

        throw new ArgumentException("Invalid endianness value", nameof(endianness));
    }

    /// <summary>
    /// Converts the given bytes to ushort in the system endianness
    /// </summary>
    /// <param name="b">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 2</exception>
    public static unsafe ushort ToUInt16(ReadOnlySpan<byte> b)
    {
        if (b.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(b));

        fixed (byte* ptr = b)
            return *(ushort*)ptr;
    }

    /// <summary>
    /// Converts the given bytes to ushort in the little-endian byte order
    /// </summary>
    /// <param name="b">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 2</exception>
    public static ushort ToUInt16LE(ReadOnlySpan<byte> b)
    {
        if (b.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(b));

        return (ushort)(b[1] << 8 | b[0]);
    }

    /// <summary>
    /// Converts the given bytes to ushort in the big-endian byte order
    /// </summary>
    /// <param name="b">bytes to convert</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown when span length is less than 2</exception>
    public static ushort ToUInt16BE(ReadOnlySpan<byte> b) => b.Length < 2
            ? throw new ArgumentException("The spab length must be at least 2", nameof(b))
            : (ushort)(b[0] << 8 | b[1]);

    /// <summary>
    /// Converts the bytes into ushort in the given byte order
    /// </summary>
    /// <param name="b">bytes to convert</param>
    /// <param name="endianness">byte order to use</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">Thrown for invalid value of endianness</exception>
    public static ushort ToUInt16(ReadOnlySpan<byte> b, Endianness endianness) => endianness switch
    {
        Endianness.Default => ToUInt16(b),
        Endianness.Big => ToUInt16BE(b),
        Endianness.Little => ToUInt16LE(b),
        _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
    };

    /// <summary>
    /// Converts ushort into bytes in the system byte order
    /// </summary>
    /// <param name="output">Where to write the bytes</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">throw when the span length is less than 2</exception>
    public static unsafe void FromUInt16(Span<byte> output, ushort value)
    {
        if (output.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(output));

        fixed (byte* ptr = output)
            *(ushort*)ptr = value;
    }

    /// <summary>
    /// Converts ushort to bytes in the little-endian byte order
    /// </summary>
    /// <param name="output">resulting bytes</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 2</exception>
    public static void FromUInt16LE(Span<byte> output, ushort value)
    {
        if (output.Length < 2)
            throw new ArgumentException("The span length must be at least 2", nameof(output));

        unchecked
        {
            output[1] = (byte)(value >> 8);
            output[0] = (byte)value;
        }
    }

    /// <summary>
    /// Converts ushort to bytes in the big-endian byte order
    /// </summary>
    /// <param name="output">resulting bytes</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 2</exception>
    public static void FromUInt16BE(Span<byte> output, ushort value)
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
    /// Converts ushort to bytes in the given byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <param name="endianness">endianness to use</param>
    /// <exception cref="ArgumentException">thrown for invalid endianness values</exception>
    public static void FromUInt16(Span<byte> output, ushort value, Endianness endianness)
    {
        switch (endianness)
        {
            case Endianness.Default:
                FromUInt16(output, value);
                return;
            case Endianness.Little:
                FromUInt16LE(output, value);
                return;
            case Endianness.Big:
                FromUInt16BE(output, value);
                return;
        }

        throw new ArgumentException("Invalid endianness value", nameof(endianness));
    }

    /// <summary>
    /// Converts bytes to int in the system byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static unsafe int ToInt32(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(data));

        fixed (byte* ptr = data)
            return *(int*)ptr;
    }

    /// <summary>
    /// Converts bytes to int in the little-endian byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">throw when the span length is less than 4</exception>
    public static int ToInt32LE(ReadOnlySpan<byte> data) => data.Length < 4
        ? throw new ArgumentException("The span length must be at least 4", nameof(data))
        : data[3] << 24 | data[2] << 16 | data[1] << 8 | data[0];

    /// <summary>
    /// Converts bytes to int in the big-endian byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">throw when the span length is less than 4</exception>
    public static int ToInt32BE(ReadOnlySpan<byte> data) => data.Length < 4
        ? throw new ArgumentException("The span length must be at least 4", nameof(data))
        : data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];

    /// <summary>
    /// Converts bytes to int in the given byte order
    /// </summary>
    /// <param name="data">data to convert</param>
    /// <param name="endianness">endianness to use</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown for invalid endianness values</exception>
    public static int ToInt32(ReadOnlySpan<byte> data, Endianness endianness) => endianness switch
    {
        Endianness.Default => ToInt32(data),
        Endianness.Little => ToInt32LE(data),
        Endianness.Big => ToInt32BE(data),
        _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
    };

    /// <summary>
    /// Converts int to bytes in the system endianness
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static unsafe void FromInt32(Span<byte> output, int value)
    {
        if (output.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(output));

        fixed (byte* ptr = output)
            *(int*)ptr = value;
    }

    /// <summary>
    /// Converts int to bytes in the little-endian byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static void FromInt32LE(Span<byte> output, int value)
    {
        if (output.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(output));

        unchecked
        {
            output[3] = (byte)(value >> 24);
            output[2] = (byte)(value >> 16);
            output[1] = (byte)(value >> 8);
            output[0] = (byte)value;
        }
    }

    /// <summary>
    /// Converts int to bytes in the big-endian byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static void FromInt32BE(Span<byte> output, int value)
    {
        if (output.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(output));

        unchecked
        {
            output[0] = (byte)(value >> 24);
            output[1] = (byte)(value >> 16);
            output[2] = (byte)(value >> 8);
            output[3] = (byte)value;
        }
    }

    /// <summary>
    /// Converts in to bytes in the given byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">int to convert</param>
    /// <param name="endianness">byte order</param>
    /// <exception cref="ArgumentException">thrown for invalid endianness</exception>
    public static void FromInt32(Span<byte> output, int value, Endianness endianness)
    {
        switch (endianness)
        {
            case Endianness.Default:
                FromInt32(output, value);
                return;
            case Endianness.Little:
                FromInt32LE(output, value);
                return;
            case Endianness.Big:
                FromInt32BE(output, value);
                return;
        }

        throw new ArgumentException("Invalid endianness value", nameof(endianness));
    }

    /// <summary>
    /// Converts bytes to uint in the system byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static unsafe uint ToUInt32(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(data));

        fixed (byte* ptr = data)
            return *(uint*)ptr;
    }

    /// <summary>
    /// Converts bytes to uint in the little-endian byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">throw when the span length is less than 4</exception>
    public static uint ToUInt32LE(ReadOnlySpan<byte> data) => data.Length < 4
        ? throw new ArgumentException("The span length must be at least 4", nameof(data))
        : unchecked((uint)(data[3] << 24 | data[2] << 16 | data[1] << 8 | data[0]));

    /// <summary>
    /// Converts bytes to uint in the big-endian byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>converted value</returns>
    /// <exception cref="ArgumentException">throw when the span length is less than 4</exception>
    public static uint ToUInt32BE(ReadOnlySpan<byte> data) => data.Length < 4
        ? throw new ArgumentException("The span length must be at least 4", nameof(data))
        : unchecked((uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]));

    /// <summary>
    /// Converts bytes to uint in the given byte order
    /// </summary>
    /// <param name="data">data to convert</param>
    /// <param name="endianness">endianness to use</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown for invalid endianness values</exception>
    public static uint ToUInt32(ReadOnlySpan<byte> data, Endianness endianness) => endianness switch
    {
        Endianness.Default => ToUInt32(data),
        Endianness.Little => ToUInt32LE(data),
        Endianness.Big => ToUInt32BE(data),
        _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
    };

    /// <summary>
    /// Converts uint to bytes in the system endianness
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static unsafe void FromUInt32(Span<byte> output, uint value)
    {
        if (output.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(output));

        fixed (byte* ptr = output)
            *(uint*)ptr = value;
    }

    /// <summary>
    /// Converts uint to bytes in the little-endian byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static void FromUInt32LE(Span<byte> output, uint value)
    {
        if (output.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(output));

        unchecked
        {
            output[3] = (byte)(value >> 24);
            output[2] = (byte)(value >> 16);
            output[1] = (byte)(value >> 8);
            output[0] = (byte)value;
        }
    }

    /// <summary>
    /// Converts uint to bytes in the big-endian byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">value to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 4</exception>
    public static void FromUInt32BE(Span<byte> output, uint value)
    {
        if (output.Length < 4)
            throw new ArgumentException("The span length must be at least 4", nameof(output));

        unchecked
        {
            output[0] = (byte)(value >> 24);
            output[1] = (byte)(value >> 16);
            output[2] = (byte)(value >> 8);
            output[3] = (byte)value;
        }
    }

    /// <summary>
    /// Converts uint to bytes in the given byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">uint to convert</param>
    /// <param name="endianness">byte order</param>
    /// <exception cref="ArgumentException">thrown for invalid endianness</exception>
    public static void FromUInt32(Span<byte> output, uint value, Endianness endianness)
    {
        switch (endianness)
        {
            case Endianness.Default:
                FromUInt32(output, value);
                return;
            case Endianness.Little:
                FromUInt32LE(output, value);
                return;
            case Endianness.Big:
                FromUInt32BE(output, value);
                return;
        }

        throw new ArgumentException("Invalid endianness value", nameof(endianness));
    }

    /// <summary>
    /// Converts bytes to long in the system byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 8</exception>
    public static unsafe long ToInt64(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
            throw new ArgumentException("The span must have length of at least 8", nameof(data));

        fixed (byte* ptr = data)
            return *(long*)ptr;
    }

    /// <summary>
    /// Converts bytes to long in the little-endian byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 8</exception>
    public static long ToInt64LE(ReadOnlySpan<byte> data) => data.Length < 8
        ? throw new ArgumentException("The span length must be at least 8", nameof(data))
        : (long)data[7] << 56 | (long)data[6] << 48 | (long)data[5] << 40 | (long)data[4] << 32 | (long)data[3] << 24 | (long)data[2] << 16 | (long)data[1] << 8 | data[0];

    /// <summary>
    /// Converts bytes to long in the big-endian byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown when the span length is less than 8</exception>
    public static long ToInt64BE(ReadOnlySpan<byte> data) => data.Length < 8
        ? throw new ArgumentException("The span length must be at least 8", nameof(data))
        : (long)data[0] << 56 | (long)data[1] << 48 | (long)data[2] << 40 | (long)data[3] << 32 | (long)data[4] << 24 | (long)data[5] << 16 | (long)data[6] << 8 | data[7];

    /// <summary>
    /// Converts bytes to long in the specified byte order
    /// </summary>
    /// <param name="data">bytes to convert</param>
    /// <param name="endianness">byte order</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown for invalid endianness values</exception>
    public static long ToInt64(ReadOnlySpan<byte> data, Endianness endianness) => endianness switch
    {
        Endianness.Default => ToInt64(data),
        Endianness.Little => ToInt64LE(data),
        Endianness.Big => ToInt64BE(data),
        _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
    };

    /// <summary>
    /// Converts long to bytes in the system byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">long to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 8</exception>
    public static unsafe void FromInt64(Span<byte> output, long value)
    {
        if (output.Length < 8)
            throw new ArgumentException("The span length must be at least 8", nameof(output));

        fixed (byte* ptr = output)
            *(long*)ptr = value;
    }

    /// <summary>
    /// Converts long to bytes in the little-endian byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">long to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 8</exception>
    public static void FromInt64LE(Span<byte> output, long value)
    {
        if (output.Length < 8)
            throw new ArgumentException("The span length must be at least 8", nameof(output));

        unchecked
        {
            output[7] = (byte)(value >> 56);
            output[6] = (byte)(value >> 48);
            output[5] = (byte)(value >> 40);
            output[4] = (byte)(value >> 32);
            output[3] = (byte)(value >> 24);
            output[2] = (byte)(value >> 16);
            output[1] = (byte)(value >> 8);
            output[0] = (byte)value;
        }
    }

    /// <summary>
    /// Converts long to bytes in the big-endian byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">long to convert</param>
    /// <exception cref="ArgumentException">thrown when the span length is less than 8</exception>
    public static void FromInt64BE(Span<byte> output, long value)
    {
        if (output.Length < 8)
            throw new ArgumentException("The span length must be at least 8", nameof(output));

        unchecked
        {
            output[0] = (byte)(value >> 56);
            output[1] = (byte)(value >> 48);
            output[2] = (byte)(value >> 40);
            output[3] = (byte)(value >> 32);
            output[4] = (byte)(value >> 24);
            output[5] = (byte)(value >> 16);
            output[6] = (byte)(value >> 8);
            output[7] = (byte)value;
        }
    }

    /// <summary>
    /// Converts long to bytes in the given byte order
    /// </summary>
    /// <param name="output">result</param>
    /// <param name="value">long to convert</param>
    /// <param name="endianness">byte order</param>
    /// <exception cref="ArgumentException">throw for invalid endianness values</exception>
    public static void FromInt64(Span<byte> output, long value, Endianness endianness)
    {
        switch (endianness)
        {
            case Endianness.Default:
                FromInt64(output, value);
                return;
            case Endianness.Little:
                FromInt64LE(output, value);
                return;
            case Endianness.Big:
                FromInt64BE(output, value);
                return;
        }

        throw new ArgumentException("Invalid endianness value");
    }
}
