using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Bny.RawBytes;

public static partial class Bytes
{
    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <typeparam name="T">type to convert to</typeparam>
    /// <param name="data">Stream to read from</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown for unsuported types
    /// </exception>
    public static T To<T>(Stream data, BytesParam? par = null)
        => (T)To(data, typeof(T), par);

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <param name="data">Stream to read from</param>
    /// <param name="type">type to convert to</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>The byte span converted to the type</returns>
    /// <exception cref="ArgumentException">T
    /// hrown for unsuported types
    /// </exception>
    public static object To(Stream data, Type type, BytesParam? par = null)
    {
        if (TryTo(data, type, out var ret, par))
            return ret;

        throw new ArgumentException(
            "Cannot convert to this value type from stream",
            nameof(type));
    }

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <typeparam name="T">type to convert to</typeparam>
    /// <param name="data">Stream to read from</param>
    /// <param name="result">the result od the operation</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryTo<T>(Stream data,
        [NotNullWhen(true)] out T? result,
                                BytesParam? par = null)
    {
        var ret = TryTo(data, typeof(T), out var res, par);
        result = (T?)res;
        return ret;
    }

    /// <summary>
    /// Reads the given type from the given stream
    /// </summary>
    /// <param name="data">Stream to read from</param>
    /// <param name="type">Type to convert to</param>
    /// <param name="result">Result of the conversion</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryTo(Stream data,
                                Type type,
        [NotNullWhen(true)] out object? result,
                                BytesParam? par = null)
    {
        par ??= new();
        par.Type = type;
        return TryTo(data, out result, par);
    }

    private static bool TryTo(Stream data,
        [NotNullWhen(true)] out object? result,
                                BytesParam par)
    {
        if (TryReadBasicStream(data, out result, par))
            return true;
        if (TryReadBinaryObjectAttribute(data, out result, par))
            return true;
        if (TryReadIBinaryObject(data, out result, par))
            return true;
        if (TryReadIBinaryInteger(data, out result, par))
            return true;
        return false;
    }

    private static bool TryReadBasicStream(
                                Stream data,
        [NotNullWhen(true)] out object? result,
                                BytesParam par)
    {
        if (par.Type == typeof(string))
        {
            result = par.GetString(data);
            return result is not null;
        }

        if (par.Type.IsEnum)
        {
            var newPar = par with { Type = par.Type.GetEnumUnderlyingType() };
            var res = TryTo(data, out var obj, newPar);

            if (!res)
            {
                result = null;
                return false;
            }

            result = Enum.ToObject(par.Type, obj);
            return true;
        }

        result = par.CreateInstance();
        if (result is null)
            return false;

        bool ret;
        switch (result)
        {
            case sbyte:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out sbyte n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case byte:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out byte n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            case short:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out short n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case ushort:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out ushort n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            case int:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out int n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case uint:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out uint n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            case long:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out long n,
                        par,
                        true);
                    result = n;
                    return ret;
                }
            case ulong:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data,
                        out ulong n,
                        par,
                        false);
                    result = n;
                    return ret;
                }
            default:
                return false;
        };
    }

    private static bool TryReadBinaryObjectAttribute(
                                Stream data,
        [NotNullWhen(true)] out object? result,
                                BytesParam par)
    {
        result = null;
        if (!TryExtractAttribute(par, out _, out var membs, out var objPar))
            return false;

        result = par.CreateInstance();
        if (result is null)
            return false;

        foreach (var m in membs)
        {
            object? res;
            switch (m.Attrib)
            {
                case BinaryMemberAttribute bma:
                    if (bma.Size >= 0)
                    {
                        Span<byte> buffer = new byte[bma.Size];
                        if (data.Read(buffer) != buffer.Length)
                            return false;
                        if (TryTo(buffer, out res, m.CreatePar(bma, objPar)) < 0)
                            return false;
                    }
                    else
                    {
                        if (!TryTo(data, out res, m.CreatePar(bma, objPar)))
                            return false;
                    }

                    m.SetValue(result, res);
                    break;
                case BinaryPaddingAttribute bpa:
                    if (bpa.Size == 0)
                        break;
                    if (bpa.Size < 0)
                        return false;
                    if (!data.CanSeek)
                    {
                        Span<byte> buffer = new byte[bpa.Size];
                        data.Read(buffer);
                        break;
                    }
                    data.Seek(bpa.Size, SeekOrigin.Current);
                    break;
                case BinaryExactAttribute bea:
                    {
                        var encoding =
                            BinaryEncoding.TryGet(bea.DataEncoding);

                        if (encoding is null)
                            return false;

                        ReadOnlySpan<byte> match =
                            encoding.GetBytes(bea.Data);

                        Span<byte> buffer = new byte[match.Length];
                        if (data.Read(buffer) != match.Length)
                            return false;

                        if (!buffer.StartsWith(match))
                            return false;
                        break;
                    }
                default:
                    return false;
            }
        }
        return true;
    }

    private static bool TryReadIBinaryObject(
                                Stream data,
        [NotNullWhen(true)] out object? result,
                                BytesParam par)
    {
        result = null;

        if (!par.Type.GetInterfaces().Any(
            p => p.FullName is not null &&
                 p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return false;

        var parm = new object[] { data, null!, par.Endianness };

        var ret = (bool)typeof(Bytes).GetMethod(
                nameof(TryReadIBinaryObjectSWrapper),
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;

        result = parm[1];
        return ret && result is not null;
    }

    private static bool TryReadIBinaryInteger(
                                Stream data,
        [NotNullWhen(true)] out object? result,
                                BytesParam par)
    {
        var buffer = new byte[128];
        using MemoryStream ms = new();

        int len;
        do ms.Write(buffer, 0, len = data.Read(buffer));
        while (len == buffer.Length);

        return TryReadIBinaryInteger(
            ms.GetBuffer().AsSpan()[..(int)ms.Position],
            out result, par) == ms.Position;
    }

    // Wrappers for IBinaryObject TryRead methods
    private static bool TryReadIBinaryObjectSWrapper<T>(
            Stream str,
        out T? result,
            Endianness endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(str, out result, endianness);

    private static bool TryReadIBinaryIntegerStream<T>(
                                Stream str,
        [NotNullWhen(true)] out T? result,
                                BytesParam par,
                                bool signed)
        where T : IBinaryInteger<T>
    {
        int size = Marshal.SizeOf<T>();
        Span<byte> buffer = stackalloc byte[size];
        str.Read(buffer);

        return TryReadIBinaryIntegerSpan(
            buffer,
            out result,
            par,
            signed) >= 0;
    }
}
