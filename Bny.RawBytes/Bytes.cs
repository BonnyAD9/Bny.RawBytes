using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Schema;

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
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(ReadOnlySpan<byte> data, Type type, out int readedBytes, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        object? res;

        if ((readedBytes = TryReadBasicSpan(data, out res, type, endianness, signed)) >= 0)
            return res!;
        if ((readedBytes = TryReadBinaryObjectAttribute(data, out res, type, endianness)) >= 0)
            return res!;
        if ((readedBytes = TryReadIBinaryObject(data, out res, type, endianness)) >= 0)
            return res!;
        if ((readedBytes = TryReadIBinaryInteger(data, out res, type, endianness, signed)) >= 0)
            return res!;
        throw new ArgumentException("Cannot convert to this value type", nameof(type));
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
            m.SetValue(result, To(data, m.MemberType, out int rb, end));
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
            Endianness.Big => "_TryReadIBinaryIntegerLE",
            Endianness.Little => "_TryReadIBinaryIntegerBE",
            Endianness.Default => IsDefaultLE ? "_TryReadIBinaryIntegerLE" : "_TryReadIBinaryIntegerBE",
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
        var ret = (int)typeof(Bytes).GetMethod("_TryReadIBinaryObject", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
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
    /// <param name="type">Type to convert to</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type, some types might ignore this</param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(Stream data, Type type, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        object? obj;
        if (TryReadBasicStream(data, out obj, type, endianness, signed))
            return obj;
        if (TryReadIBinaryObject(data, out obj, type, endianness))
            return obj;
        throw new ArgumentException("Cannot convert to this value type from stream", nameof(type));
    }

    private static bool TryReadIBinaryObject(Stream data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness)
    {
        result = null;

        if (!type.GetInterfaces().Any(p => p.FullName is not null && p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return false;

        var parm = new object[] { data, null!, endianness };
        var ret = (bool)typeof(Bytes).GetMethod("_TryReadIBinaryObjectS", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
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
    /// <param name="value">vylue to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>number of written bytes, -1 on error</returns>
    public static int From<T>(T value, Span<byte> result, Endianness endianness = Endianness.Default)
        => From(value!, result, typeof(T), endianness);

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <param name="value">vylue to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="type">Type of the value to convert</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>number of written bytes, -1 on error</returns>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static int From(object value, Span<byte> result, Type type, Endianness endianness = Endianness.Default)
    {
        int len;
        if ((len = TryWriteAttribute(value, result, type, endianness)) >= 0)
            return len;

        if (value is IBinaryObjectWrite bow)
        {
            int count = bow.TryWriteToBinary(result, endianness);
            if (count > 0)
                return count;
        }

        if ((len = TryWriteIBinaryInteger(value, result, type, endianness)) >= 0)
            return len;
        throw new ArgumentException("Cannot convert from this value type", nameof(value));
    }

    private static int TryWriteAttribute(object value, Span<byte> result, Type type, Endianness endianness)
    {
        if (!TryExtractAttribute(type, out var attrib, ref endianness, out var members))
            return -1;

        int bytesWritten = 0;
        foreach (var m in members)
        {
            var end = m.Attrib.Endianness == Endianness.Default ? endianness : m.Attrib.Endianness;
            var wb = From(m.GetValue(value)!, result, m.MemberType, end);
            result = result[wb..];
            bytesWritten += wb;
        }

        return bytesWritten;
    }

    private static int TryWriteIBinaryInteger(object data, Span<byte> result, Type type, Endianness endianness)
    {
        string mname = endianness switch
        {
            Endianness.Big => "_TryWriteIBinaryIntegerBE",
            Endianness.Little => "_TryWriteIBinaryIntegerLE",
            Endianness.Default => IsDefaultLE ? "_TryWriteIBinaryIntegerLE" : "_TryWriteIBinaryIntegerBE",
            _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
        };

        var bi = type.GetInterfaces().FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (bi is null)
            return -1;

        try
        {
            int byteCount = (int)bi.GetMethod("GetByteCount", Array.Empty<Type>())!.Invoke(data, Array.Empty<object>())!;
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
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static bool From<T>(T value, Stream output, Endianness endianness = Endianness.Default)
        => From(value!, output, typeof(T), endianness);

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <param name="value">vylue to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="type">Type of the value to convert</param>
    /// <param name="endianness">Byte order of the conversion</param>
    /// <returns>the converted value</returns>
    /// <exception cref="ArgumentException">thrown for unsupported types</exception>
    public static bool From(object value, Stream output, Type type, Endianness endianness = Endianness.Default)
    {
        int size = -1;
        byte[] buffer;

        if (value is IBinaryObjectWrite bow)
        {
            size = bow.WriteSize;
            buffer = new byte[size];
            bow.TryWriteToBinary(buffer, endianness);
            output.Write(buffer);
            return true;
        }

        var interfaces = type.GetInterfaces();

        var intf = interfaces.FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (intf is not null)
            size = (int)intf.GetMethod("GetByteCount")!.Invoke(value, Array.Empty<object>())!;

        if (size == -1)
            return false;

        buffer = new byte[size];
        var ret = From(value, buffer, type, endianness) >= 0;
        if (!ret)
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
