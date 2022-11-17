using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

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
    /// The default byte order
    /// </summary>
    public static readonly Endianness DefaultEndianness = IsDefaultLE ? Endianness.Little : Endianness.Big;

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type</param>
    /// <returns>The byte span converted to the type</returns>
    public static T To<T>(ReadOnlySpan<byte> data, Endianness endianness = Endianness.Default, bool? signed = null)
        => (T)To(data, typeof(T), endianness, signed);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="readedBytes">number of bytes readed</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type</param>
    /// <returns>The byte span converted to the type</returns>
    public static T To<T>(ReadOnlySpan<byte> data, out int readedBytes, Endianness endianness = Endianness.Default, bool? signed = null)
        => (T)To(data, typeof(T), out readedBytes, endianness, signed);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data">Bytes to convert</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(ReadOnlySpan<byte> data, Type type, Endianness endianness = Endianness.Default, bool? signed = null)
        => To(data, type, out _, endianness, signed);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data">Bytes to convert</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="readedBytes">number of bytes readed</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type</param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(ReadOnlySpan<byte> data, Type type, out int readedBytes, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        if ((readedBytes = TryTo(data, type, out var ret, endianness, signed)) >= 0)
            return ret!;
        throw new ArgumentException("Cannot convert to this value type from stream", nameof(type));
    }

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="result">the result, not null when returns positive</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type</param>
    /// <returns>The byte span converted to the type</returns>
    public static int TryTo<T>(ReadOnlySpan<byte> data, out T? result, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        var ret = TryTo(data, typeof(T), out var res, endianness, signed);
        result = (T?)res;
        return ret;
    }

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data">Bytes to convert</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="result">the converted value</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>number of readed bytes on success, otherwise negative</returns>
    public static int TryTo(ReadOnlySpan<byte> data, Type type, out object? result, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        int rb;

        if ((rb = TryReadBasicSpan(data, out result, type, endianness, signed)) >= 0)
            return rb;
        if ((rb = TryReadBinaryObjectAttribute(data, out result, type, endianness)) >= 0)
            return rb;
        if ((rb = TryReadIBinaryObject(data, out result, type, endianness)) >= 0)
            return rb;
        if ((rb = TryReadIBinaryInteger(data, out result, type, endianness, signed)) >= 0)
            return rb;
        return -1;
    }

    private static int TryReadBinaryObjectAttribute(ReadOnlySpan<byte> data, out object? result, Type type, Endianness endianness)
    {
        result = null;
        if (!TryExtractAttribute(type, out var attrib, ref endianness, out var members))
            return -1;

        result = CreateInstance(type);
        if (result is null)
            return -1;

        int totalReaded = 0;

        foreach (var m in members)
        {
            var end = m.Attrib.Endianness == Endianness.Default ? endianness : m.Attrib.Endianness;
            int rb = TryTo(data, m.MemberType, out var res, end);
            if (rb < 0)
                return -1;

            m.SetValue(result, res); // TODO: add TrySetValue
            data = data[rb..];
            totalReaded += rb;
        }

        return totalReaded;
    }

    private static int TryReadIBinaryInteger(ReadOnlySpan<byte> data, out object? result, Type type, Endianness endianness, bool? signed)
    {
        result = null;
        string mname = endianness switch
        {
            Endianness.Big => nameof(_TryReadIBinaryIntegerBE),
            Endianness.Little => nameof(_TryReadIBinaryIntegerLE),
            Endianness.Default => IsDefaultLE ? nameof(_TryReadIBinaryIntegerLE) : nameof(_TryReadIBinaryIntegerBE),
            _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
        };

        // check whether the type implements the IBinaryInteger interface
        var interfaces = type.GetInterfaces();
        if (!interfaces.Any(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger")))
            return -1;

        // if the signed value is not set, set it based on whether the type implements ISignedNumber
        bool isUnsigned = signed.HasValue ? !signed.Value : !interfaces.Any(p => p.FullName is not null && p.FullName.Contains("System.Numerics.ISignedNumber"));

        // use reflection to call wrappers for IBinaryInteger.TryReadLittleEndian or IBinaryInteger.TryReadBigEndian with the type parameter
        var parm = new object[] { new SizedPointer<byte>(data), isUnsigned, null! };
        var res = (bool)typeof(Bytes).GetMethod(mname, BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
        result = parm[2];
        res = res && result is not null;
        return res ? data.Length : -1;
    }

    private static int TryReadIBinaryObject(ReadOnlySpan<byte> data, out object? result, Type type, Endianness endianness)
    {
        result = null;

        // check whether the type implements the IBinaryObject interface
        if (!type.GetInterfaces().Any(p => p.FullName is not null && p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return -1;

        // use reflection to call the generic wrapper
        var parm = new object[] { new SizedPointer<byte>(data), null!, endianness };
        var ret = (int)typeof(Bytes).GetMethod(nameof(_TryReadIBinaryObject), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
        result = parm[1];
        ret = result is null && ret >= 0 ? -1 : ret;
        return ret;
    }

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <typeparam name="T">type to convert to</typeparam>
    /// <param name="data">Stream to read from</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static T To<T>(Stream data, Endianness endianness = Endianness.Default, bool? signed = null)
        => (T)To(data, typeof(T), endianness, signed);

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <param name="data">Stream to read from</param>
    /// <param name="type">type to convert to</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(Stream data, Type type, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        if (TryTo(data, type, out var ret, endianness, signed))
            return ret;
        throw new ArgumentException("Cannot convert to this value type from stream", nameof(type));
    }

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <typeparam name="T">type to convert to</typeparam>
    /// <param name="data">Stream to read from</param>
    /// <param name="result">the result od the operation</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryTo<T>(Stream data, [NotNullWhen(true)] out T? result, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        var ret = TryTo(data, typeof(T), out var res, endianness, signed);
        result = (T?)res;
        return ret;
    }

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <param name="data">Stream to read from</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="result">Result of the conversion</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryTo(Stream data, Type type, [NotNullWhen(true)] out object? result, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        if (TryReadBasicStream(data, out result, type, endianness, signed))
            return true;
        if (TryReadBinaryObjectAttribute(data, out result, type, endianness))
            return true;
        if (TryReadIBinaryObject(data, out result, type, endianness))
            return true;
        return false;
    }

    private static bool TryReadBinaryObjectAttribute(Stream data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness)
    {
        result = null;
        if (!TryExtractAttribute(type, out var attrib, ref endianness, out var membs))
            return false;

        result = CreateInstance(type);
        if (result is null)
            return false;

        foreach (var m in membs)
        {
            var end = m.Attrib.Endianness == Endianness.Default ? endianness : m.Attrib.Endianness;
            if (!TryTo(data, m.MemberType, out var res, end))
                return false;
            m.SetValue(result, res);
        }
        return true;
    }

    private static bool TryReadIBinaryObject(Stream data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness)
    {
        result = null;

        if (!type.GetInterfaces().Any(p => p.FullName is not null && p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return false;

        var parm = new object[] { data, null!, endianness };
        var ret = (bool)typeof(Bytes).GetMethod(nameof(_TryReadIBinaryObjectS), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
        result = parm[1];
        return ret && result is not null;
    }

    private static bool TryReadBasicStream(Stream data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness, bool? signed = null)
    {
        result = CreateInstance(type);
        if (result is null)
            return false;

        bool ret;
        switch (result)
        {
            case sbyte n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case byte n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            case short n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case ushort n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            case int n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case uint n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            case long n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case ulong n:
                ret = TryReadIBinaryIntegerStream(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            default:
                return false;
        };
    }
    private static int TryReadBasicSpan(ReadOnlySpan<byte> data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness, bool? signed = null)
    {
        result = CreateInstance(type);
        if (result is null)
            return -1;

        int ret;
        switch (result)
        {
            case sbyte n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case byte n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            case short n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case ushort n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            case int n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case uint n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            case long n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? true);
                result = n;
                return ret;
            case ulong n:
                ret = TryReadIBinaryIntegerSpan(data, out n, endianness, signed ?? false);
                result = n;
                return ret;
            default:
                return -1;
        };
    }

    // Wrappers for IBinaryInteger TryRead methods with SizedPointer as parameter instead of Span
    private static bool _TryReadIBinaryIntegerLE<T>(SizedPointer<byte> ptr, bool isUnsigned, out T result) where T : IBinaryInteger<T>
        => T.TryReadLittleEndian(ptr, isUnsigned, out result);
    private static bool _TryReadIBinaryIntegerBE<T>(SizedPointer<byte> ptr, bool isUnsigned, out T result) where T : IBinaryInteger<T>
        => T.TryReadBigEndian(ptr, isUnsigned, out result);

    // Wrapper for IBinaryObject TryRead methods
    private static int _TryReadIBinaryObject<T>(SizedPointer<byte> ptr, out T? result, Endianness endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(ptr, out result, endianness);
    private static bool _TryReadIBinaryObjectS<T>(Stream str, out T? result, Endianness endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(str, out result, endianness);

    private static int TryReadIBinaryIntegerSpan<T>(ReadOnlySpan<byte> span, [NotNullWhen(true)] out T? result, Endianness endianness, bool signed) where T : IBinaryInteger<T>
    {
        result = default;
        int size = Marshal.SizeOf<T>();
        span = span[..size];

        if (endianness switch
        {
            Endianness.Big => T.TryReadBigEndian(span, !signed, out result),
            Endianness.Little => T.TryReadLittleEndian(span, !signed, out result),
            Endianness.Default => IsDefaultLE ? T.TryReadLittleEndian(span, !signed, out result) : T.TryReadBigEndian(span, !signed, out result),
            _ => false,
        })
            return size;
        return -1;
    }

    private static bool TryReadIBinaryIntegerStream<T>(Stream str, [NotNullWhen(true)] out T? result, Endianness endianness, bool signed) where T : IBinaryInteger<T>
    {
        int size = Marshal.SizeOf<T>();
        Span<byte> buffer = stackalloc byte[size];
        str.Read(buffer);

        return TryReadIBinaryIntegerSpan(buffer, out result, endianness, signed) >= 0;
    }

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>number of written bytes</returns>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static int From<T>(T? value, Span<byte> result, Endianness endianness = Endianness.Default)
    {
        var ret = TryFrom(value, result, typeof(T), endianness);
        return ret < 0
            ? throw new ArgumentException("Cannot convert to this value type", nameof(value))
            : ret;
    }

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>number of written bytes</returns>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static int From(object? value, Span<byte> result, Endianness endianness = Endianness.Default)
    {
        var ret = TryFrom(value, result, endianness);
        return ret < 0
            ? throw new ArgumentException("Cannot convert to this value type", nameof(value))
            : ret;
    }

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>number of written bytes, -1 on error</returns>
    public static int TryFrom<T>(T? value, Span<byte> result, Endianness endianness = Endianness.Default)
        => TryFrom(value, result, typeof(T), endianness);

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>number of written bytes, -1 on error</returns>
    public static int TryFrom(object? value, Span<byte> result, Endianness endianness = Endianness.Default)
        => TryFrom(value, result, value?.GetType()!, endianness);

    private static int TryFrom(object? value, Span<byte> result, Type type, Endianness endianness)
    {
        if (value is null)
            return -1;
        int len;
        if ((len = TryWriteBinaryAttribute(value, result, type, endianness)) >= 0)
            return len;
        if ((len = TryWriteIBinaryObjectWrite(value, result, endianness)) >= 0)
            return len;
        if ((len = TryWriteIBinaryInteger(value, result, type, endianness)) >= 0)
            return len;
        return -1;
    }

    private static int TryWriteIBinaryObjectWrite(object value, Span<byte> result, Endianness endianness = Endianness.Default)
    {
        if (value is not IBinaryObjectWrite bow)
            return -1;
        return bow.TryWriteToBinary(result, endianness);
    }

    private static int TryWriteBinaryAttribute(object value, Span<byte> result, Type type, Endianness endianness)
    {
        if (!TryExtractAttribute(type, out var attrib, ref endianness, out var members))
            return -1;

        int bytesWritten = 0;
        foreach (var m in members)
        {
            var end = m.Attrib.Endianness == Endianness.Default ? endianness : m.Attrib.Endianness;
            var wb = TryFrom(m.GetValue(value)!, result, m.MemberType, end);
            if (wb < 0)
                return -1;
            result = result[wb..];
            bytesWritten += wb;
        }

        return bytesWritten;
    }

    private static int TryWriteIBinaryInteger(object data, Span<byte> result, Type type, Endianness endianness)
    {
        string mname = endianness switch
        {
            Endianness.Big => nameof(_TryWriteIBinaryIntegerBE),
            Endianness.Little => nameof(_TryWriteIBinaryIntegerLE),
            Endianness.Default => IsDefaultLE ? nameof(_TryWriteIBinaryIntegerLE) : nameof(_TryWriteIBinaryIntegerBE),
            _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
        };

        var bi = type.GetInterfaces().FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (bi is null)
            return -1;

        try
        {
            int byteCount = (int)bi.GetMethod(nameof(IBinaryInteger<int>.GetByteCount), Array.Empty<Type>())!.Invoke(data, Array.Empty<object>())!;
            if (result.Length < byteCount)
                return -1;

            var parm = new object[] { data, new SizedPointer<byte>(result), null! };
            var res = (bool)typeof(Bytes).GetMethod(mname, BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;

            return res ? (int)parm[2] : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">vylue to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static void From<T>(T? value, Stream output, Endianness endianness = Endianness.Default)
    {
        if (!TryFrom(value, output, typeof(T), endianness))
            throw new ArgumentException("Cannot convert to this value type from stream", nameof(value));
    }

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <param name="value">vylue to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static void From(object? value, Stream output, Endianness endianness = Endianness.Default)
    {
        if (!TryFrom(value, output, value?.GetType()!, endianness))
            throw new ArgumentException("Cannot convert to this value type from stream", nameof(value));
    }

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryFrom<T>(T? value, Stream output, Endianness endianness = Endianness.Default)
        => TryFrom(value, output, typeof(T), endianness);

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryFrom(object? value, Stream output, Endianness endianness = Endianness.Default)
        => TryFrom(value, output, value?.GetType()!, endianness);

    private static bool TryFrom(object? value, Stream output, Type type, Endianness endianness = Endianness.Default)
    {
        if (value is null)
            return false;
        if (TryWriteBinaryObjectAttribute(value, output, type, endianness))
            return true;
        if (TryWriteIBinaryObjectWrite(value, output, endianness))
            return true;
        if (TryWriteIBinaryInteger(value, output, type, endianness))
            return true;
        return false;
    }

    private static bool TryWriteBinaryObjectAttribute(object value, Stream output, Type type, Endianness endianness)
    {
        if (!TryExtractAttribute(type, out var attrib, ref endianness, out var members))
            return false;

        foreach (var m in members)
        {
            var end = m.Attrib.Endianness == Endianness.Default ? endianness : m.Attrib.Endianness;
            if (!TryFrom(m.GetValue(value)!, output, m.MemberType, end))
                return false;
        }
        return true;
    }

    private static bool TryWriteIBinaryObjectWrite(object value, Stream output, Endianness endianness)
    {
        if (value is not IBinaryObjectWrite bow)
            return false;

        var size = bow.WriteSize;
        Span<byte> buffer = new byte[size];
        bow.TryWriteToBinary(buffer, endianness);
        output.Write(buffer);
        return true;
    }

    private static bool TryWriteIBinaryInteger(object value, Stream output, Type type, Endianness endianness)
    {
        var interfaces = type.GetInterfaces();

        var intf = interfaces.FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (intf is null)
            return false;

        int size = (int)intf.GetMethod(nameof(IBinaryInteger<int>.GetByteCount))!.Invoke(value, Array.Empty<object>())!;

        Span<byte> buffer = new byte[size];
        if (TryFrom(value, buffer, type, endianness) < 0)
            return false;
        output.Write(buffer);
        return true;
    }

    private static bool _TryWriteIBinaryIntegerLE<T>(T value, SizedPointer<byte> ptr, out int bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteLittleEndian(ptr, out bytesWritten);
    private static bool _TryWriteIBinaryIntegerBE<T>(T value, SizedPointer<byte> ptr, out int bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteBigEndian(ptr, out bytesWritten);

    private static object? CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryExtractAttribute(
        Type type,
        [NotNullWhen(true)]out BinaryObjectAttribute? attrib,
        ref Endianness endianness,
        out Span<BinaryMemberAttributeInfo> members)
    {
        members = Array.Empty<BinaryMemberAttributeInfo>().AsSpan();

        attrib = type.GetCustomAttribute<BinaryObjectAttribute>();
        if (attrib is null)
            return false;

        endianness = attrib.Endianness == Endianness.Default ? (endianness == Endianness.Default ? DefaultEndianness : endianness) : attrib.Endianness;

        const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        members = type.GetFields(AllBindingFlags).Select(p => new BinaryMemberAttributeInfo(p))
            .Concat(type.GetProperties(AllBindingFlags).Select(p => new BinaryMemberAttributeInfo(p)))
            .Where(p => p.Attrib is not null).OrderBy(p => p.Attrib.Order).ToArray().AsSpan();

        return true;
    }
}
