﻿using Bny.General.Memory;
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
            int rb;
            switch (m.Attrib)
            {
                case BinaryMemberAttribute bma:
                    {
                        var d = data;
                        int sizeLimit = -1;
                        if (bma.Size >= 0)
                        {
                            if (bma.Size > d.Length)
                                return -1;
                            d = d[..bma.Size];
                            sizeLimit = bma.Size;
                        }

                        rb = TryTo(d, out var res, m.CreatePar(bma, objPar));
                        if (rb < 0)
                            return -1;

                        m.SetValue(result, res);

                        rb = Math.Max(rb, sizeLimit);
                        break;
                    }
                case BinaryPaddingAttribute bpa:
                    rb = bpa.Size;
                    if (rb < 0 || rb > data.Length)
                        return -1;
                    break;
                case BinaryExactAttribute bea:
                    {
                        var encoding =
                            BinaryEncoding.TryGet(bea.DataEncoding);

                        if (encoding is null)
                            return -1;

                        ReadOnlySpan<byte> match =
                            encoding.GetBytes(bea.Data);

                        rb = match.Length;
                        if (rb > data.Length)
                            return -1;

                        if (!data.StartsWith(match))
                            return -1;
                        break;
                    }
                default:
                    return -1;
            }
            data = data[rb..];
            totalReaded += rb;
        }

        return totalReaded;
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