using Bny.General.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Bny.RawBytes;

public static partial class Bytes
{
    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    public static T To<T>(ReadOnlySpan<byte> data, BytesParam? par = null)
        => (T)To(data, typeof(T), par);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="readedBytes">number of bytes readed</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    public static T To<T>(
            ReadOnlySpan<byte> data,
        out int readedBytes,
            BytesParam? par = null)
        => (T)To(data, typeof(T), out readedBytes, par);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data">Bytes to convert</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(
        ReadOnlySpan<byte> data,
        Type type,
        BytesParam? par = null)
        => To(data, type, out _, par);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data">Bytes to convert</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="readedBytes">number of bytes readed</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">Thrown for unsuported types</exception>
    public static object To(
            ReadOnlySpan<byte> data,
            Type type,
        out int readedBytes,
            BytesParam? par = null)
    {
        if ((readedBytes = TryTo(data, type, out var ret, par)) >= 0)
            return ret!;
        throw new ArgumentException(
            "Cannot convert to this value type from stream",
            nameof(type));
    }

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="data">Bytes to convert</param>
    /// <param name="result">
    /// the result, not null when returns positive
    /// </param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    public static int TryTo<T>(
            ReadOnlySpan<byte> data,
        out T? result,
            BytesParam? par = null)
    {
        var ret = TryTo(data, typeof(T), out var res, par);
        result = (T?)res;
        return ret;
    }

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data">Bytes to convert</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="result">the converted value</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>
    /// number of readed bytes on success, otherwise negative
    /// </returns>
    public static int TryTo(
            ReadOnlySpan<byte> data,
            Type type,
        out object? result,
            BytesParam? par = null)
    {
        par ??= new();
        par.Type = type;
        return TryTo(data, out result, par);
    }

    private static int TryTo(
            ReadOnlySpan<byte> data,
        out object? result,
            BytesParam par)
    {
        int rb;

        if ((rb = TryReadBasicSpan(data, out result, par)) >= 0)
            return rb;
        if ((rb = TryReadBinaryObjectAttribute(data, out result, par)) >= 0)
            return rb;
        if ((rb = TryReadIBinaryObject(data, out result, par)) >= 0)
            return rb;
        if ((rb = TryReadIBinaryInteger(data, out result, par)) >= 0)
            return rb;
        return -1;
    }

    private static int TryReadBasicSpan(
                                ReadOnlySpan<byte> data,
        [NotNullWhen(true)] out object? result,
                                BytesParam par)
    {
        if (par.Type == typeof(string))
        {
            result = par.GetString(data, out var br);
            return result is null ? -1 : br;
        }

        if (par.Type.IsEnum)
        {
            var newPar = par with { Type = par.Type.GetEnumUnderlyingType() };
            var res = TryTo(data, out var obj, newPar);
            
            if (res < 0)
            {
                result = null;
                return res;
            }

            result = Enum.ToObject(par.Type, obj!);
            return res;
        }

        result = par.CreateInstance();
        if (result is null)
            return -1;

        int ret;
        switch (result)
        {
            case sbyte:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out sbyte n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case byte:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out byte n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            case short:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out short n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case ushort:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out ushort n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            case int:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out int n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case uint:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out uint n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            case long:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out long n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case ulong:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data,
                        out ulong n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            default:
                return -1;
        };
    }

    private static int TryReadBinaryObjectAttribute(
            ReadOnlySpan<byte> data,
        out object? result,
            BytesParam par)
    {
        result = null;
        if (!TryExtractAttribute(par, out _, out var members, out var objPar))
            return -1;

        result = par.CreateInstance();
        if (result is null)
            return -1;

        int totalReaded = 0;

        foreach (var m in members)
        {
            var rb = m.Attrib switch
            {
                BinaryMemberAttribute bma => TryReadBinaryMemberAttribute(
                    data  ,
                    result,
                    bma   ,
                    m     ,
                    objPar),
                BinaryPaddingAttribute bpa
                    => bpa.Size < 0 || bpa.Size > data.Length ? -1 : bpa.Size,
                BinaryExactAttribute bea
                    => TryReadBinaryExactAttribute(data, bea),
                ExtensionBinaryAttribute cba => TryReadExtensionBinaryAttribute(
                    data  ,
                    result,
                    cba   ,
                    m     ,
                    objPar),
                CustomBinaryAttribute cba => TryReadCustomBinaryAttribute(
                    result,
                    data  ,
                    cba   ,
                    objPar),
                _ => -1,
            };
            if (rb < 0)
                return rb;

            data = data[rb..];
            totalReaded += rb;
        }

        return totalReaded;
    }

    private static int TryReadBinaryMemberAttribute(
        ReadOnlySpan<byte>    d     ,
        object?               result,
        BinaryMemberAttribute bma   ,
        BinaryAttributeInfo   m     ,
        BytesParam            objPar)
    {
        int sizeLimit = -1;

        if (bma.Size >= 0)
        {
            if (bma.Size > d.Length)
                return -1;
            d = d[..bma.Size];
            sizeLimit = bma.Size;
        }

        int rb = TryTo(d, out var res, m.CreatePar(bma, objPar));
        if (rb < 0)
            return -1;

        m.SetValue(result, res);

        rb = Math.Max(rb, sizeLimit);
        return rb;
    }

    private static int TryReadBinaryExactAttribute(
        ReadOnlySpan<byte>   data,
        BinaryExactAttribute bea )
    {
        var encoding =
            BinaryEncoding.TryGet(bea.DataEncoding);

        if (encoding is null)
            return -1;

        ReadOnlySpan<byte> match =
            encoding.GetBytes(bea.Data);

        int rb = match.Length;
        if (rb > data.Length)
            return -1;

        if (!data.StartsWith(match))
            return -1;
        return rb;
    }

    private static int TryReadExtensionBinaryAttribute(
        ReadOnlySpan<byte>    data  ,
        object                result,
        ExtensionBinaryAttribute cba   ,
        BinaryAttributeInfo   m     ,
        BytesParam            objPar)
    {
        int rb = cba.ReadFromSpan(
            data,
            out var obj,
            objPar with { Type = m.MemberType }
        );

        if (rb < 0)
            return rb;

        m.SetValue(result, obj);
        return rb;
    }

    private static int TryReadCustomBinaryAttribute(
        object                obj  ,
        ReadOnlySpan<byte>    data ,
        CustomBinaryAttribute cba  ,
        BytesParam            param)
    {
        if (data.Length < cba.Size)
            return -1;

        return TryReadCustomBinaryAttributeWrapper(
            obj             ,
            data[..cba.Size],
            cba.ID          ,
            param           )
                ? cba.Size
                : -1;
    }

    private static unsafe int TryReadIBinaryInteger(
            ReadOnlySpan<byte> data,
        out object? result,
            BytesParam par)
    {
        result = null;
        string mname = par.GetEndianness() == Endianness.Little
            ? nameof(TryReadIBinaryIntegerLEWrapper)
            : nameof(TryReadIBinaryIntegerBEWrapper);

        // check whether the type implements the IBinaryInteger interface
        var interfaces = par.Type.GetInterfaces();
        if (!interfaces.Any(
                p => p.FullName is not null &&
                     p.FullName.Contains("System.Numerics.IBinaryInteger")))
            return -1;

        // if the signed value is not set,
        // set it based on whether the type implements ISignedNumber
        bool isUnsigned = !par.IsSigned(interfaces.Any(
            p => p.FullName is not null &&
                 p.FullName.Contains("System.Numerics.ISignedNumber")));

        bool res;
        object[] parm;
        fixed (byte* ptr = data) // ensure that the memory is fixed
        {
            // use reflection to call wrappers for
            // IBinaryInteger.TryReadLittleEndian or
            // IBinaryInteger.TryReadBigEndian with the type parameter
            parm = new object[]
            {
                new ReadOnlySpanWrapper<byte>(data), isUnsigned, null!
            };

            res = (bool)typeof(Bytes).GetMethod(
                    mname,
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;
        }

        result = parm[2];
        res = res && result is not null;
        return res ? data.Length : -1;
    }

    private static unsafe int TryReadIBinaryObject(
            ReadOnlySpan<byte> data,
        out object? result,
            BytesParam par)
    {
        result = null;

        // check whether the type implements the IBinaryObject interface
        if (!par.Type.GetInterfaces().Any(
            p => p.FullName is not null &&
                 p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return -1;

        object[] parm;
        int ret;

        fixed (byte* ptr = data) // ensure that the memory is fixed
        {
            // use reflection to call the generic wrapper
            parm = new object[]
            {
                new ReadOnlySpanWrapper<byte>(data), null!, par.Endianness
            };

            ret = (int)typeof(Bytes).GetMethod(
                    nameof(TryReadIBinaryObjectWrapper),
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;
        }

        result = parm[1];
        ret = result is null && ret >= 0 ? -1 : ret;
        return ret;
    }

    private static bool TryReadCustomBinaryAttributeWrapper(
        object?            obj  ,
        ReadOnlySpan<byte> data ,
        string?            id   ,
        BytesParam         param)
        => obj is IBinaryCustom bc && bc.TryReadCustom(data, id, param);

    // Wrappers for IBinaryInteger TryRead methods with
    // SizedPointer as parameter instead of Span
    private static bool TryReadIBinaryIntegerLEWrapper<T>(
            ReadOnlySpanWrapper<byte> ptr,
            bool isUnsigned,
        out T result) where T : IBinaryInteger<T>
        => T.TryReadLittleEndian(ptr, isUnsigned, out result);

    private static bool TryReadIBinaryIntegerBEWrapper<T>(
            ReadOnlySpanWrapper<byte> ptr,
            bool isUnsigned,
        out T result) where T : IBinaryInteger<T>
        => T.TryReadBigEndian(ptr, isUnsigned, out result);

    // Wrappers for IBinaryObject TryRead methods
    private static int TryReadIBinaryObjectWrapper<T>(
            ReadOnlySpanWrapper<byte> ptr,
        out T? result,
            Endianness endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(ptr, out result, endianness);

    private static int TryReadIBinaryIntegerSpan<T>(
                                ReadOnlySpan<byte> span,
        [NotNullWhen(true)] out T? result,
                                BytesParam par,
                                bool signed)
        where T : IBinaryInteger<T>
    {
        int size = Marshal.SizeOf<T>();
        span = span[..size];

        if (par.GetEndianness(DefaultEndianness) == Endianness.Little
                ? T.TryReadLittleEndian(span, !signed, out result)
                : T.TryReadBigEndian(span, !signed, out result))
            return size;
        return -1;
    }
}
