using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="endianness">byte order</param>
    /// <param name="signed">True if the readed value should be signed, false if not, null to depend on the type</param>
    /// <returns>The byte span converted to the type</returns>
    public static T To<T>(ReadOnlySpan<byte> data, Endianness endianness = Endianness.Default, bool? signed = null) where T : new()
        => (T)To(data, typeof(T), endianness, signed);

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
    {
        object? res;
        if (TryReadIBinaryObject(data, out res, type, endianness))
            return res;
        if (TryReadIBinaryInteger(data, out res, type, endianness, signed))
            return res;
        throw new ArgumentException("Cannot convert to this value type", nameof(type));
    }

    private static bool TryReadIBinaryInteger(ReadOnlySpan<byte> data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness, bool? signed)
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
            return false;

        // if the signed value is not set, set it based on whether the type implements ISignedNumber
        bool isUnsigned = signed.HasValue ? !signed.Value : !interfaces.Any(p => p.FullName is not null && p.FullName.Contains("System.Numerics.ISignedNumber"));

        // use reflection to call wrappers for IBinaryInteger.TryReadLittleEndian or IBinaryInteger.TryReadBigEndian with the type parameter
        var parm = new object[] { new SizedPointer<byte>(data), isUnsigned, null! };
        var res = (bool)typeof(Bytes).GetMethod(mname, BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
        result = parm[2];
        return res && result is not null;
    }

    private static bool TryReadIBinaryObject(ReadOnlySpan<byte> data, [NotNullWhen(true)] out object? result, Type type, Endianness endianness)
    {
        result = null;

        // check whether the type implements the IBinaryObject interface
        if (!type.GetInterfaces().Any(p => p.FullName is not null && p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return false;

        // use reflection to call the generic wrapper
        var parm = new object[] { new SizedPointer<byte>(data), null!, endianness };
        var ret = (bool)typeof(Bytes).GetMethod("_TryReadIBinaryObject", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type)!.Invoke(null, parm)!;
        result = parm[1];
        return ret && result is not null;
    }

    // Wrappers for IBinaryInteger TryRead methods with SizedPointer as parameter instead of Span
    private static bool _TryReadIBinaryIntegerLE<T>(SizedPointer<byte> ptr, bool isUnsigned, out T result) where T : IBinaryInteger<T>
        => T.TryReadLittleEndian(ptr, isUnsigned, out result);
    private static bool _TryReadIBinaryIntegerBE<T>(SizedPointer<byte> ptr, bool isUnsigned, out T result) where T : IBinaryInteger<T>
        => T.TryReadBigEndian(ptr, isUnsigned, out result);

    // Wrapper for IBinaryObject TryRead method
    private static bool _TryReadIBinaryObject<T>(SizedPointer<byte> ptr, out T? result, Endianness endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(ptr, out result, endianness);

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
        if (value is IBinaryObjectWrite bow)
        {
            int count = bow.TryWriteToBinary(result, endianness);
            if (count > 0)
                return count;
        }

        int len = TryWriteIBinaryInteger(value, result, type, endianness);
        if (len != -1)
            return len;
        throw new ArgumentException("Cannot convert from this value type", nameof(value));
    }

    private static int TryWriteIBinaryInteger(object data, Span<byte> result, Type type, Endianness endianness = Endianness.Default)
    {
        string mname = endianness switch
        {
            Endianness.Big => "_TryWriteIBinaryIntegerLE",
            Endianness.Little => "_TryWriteIBinaryIntegerBE",
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

    private static bool _TryWriteIBinaryIntegerLE<T>(T value, SizedPointer<byte> ptr, out int bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteLittleEndian(ptr, out bytesWritten);
    private static bool _TryWriteIBinaryIntegerBE<T>(T value, SizedPointer<byte> ptr, out int bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteBigEndian(ptr, out bytesWritten);
}
