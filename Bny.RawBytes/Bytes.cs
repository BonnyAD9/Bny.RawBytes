using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Bny.RawBytes;

/// <summary>
/// Contains functions for converting from and to byte array
/// </summary>
public static class Bytes
{
    /// <summary>
    /// Gets the default endianness.
    /// Returns <c>true</c> if the <c>Endianness.Default</c> conversion has
    /// the same results as <c>Endianness.Little</c>,
    /// otherwise returns <c>false</c>.
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
    public static readonly Endianness DefaultEndianness =
        IsDefaultLE
            ? Endianness.Little
            : Endianness.Big;

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
    public static T To<T>(ReadOnlySpan<byte> data, out int readedBytes, BytesParam? par = null)
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
        ReadOnlySpan<byte> data       ,
        Type               type       , 
        BytesParam?        par  = null)
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
            ReadOnlySpan<byte> data              ,
            Type               type              ,
        out int                readedBytes       ,
            BytesParam?        par         = null)
    {
        if ((readedBytes = TryTo(data, type, out var ret, par)) >= 0)
            return ret!;
        throw new ArgumentException(
            "Cannot convert to this value type from stream", 
            nameof(type)                                   );
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
            ReadOnlySpan<byte> data         ,
        out T?                 result       ,
            BytesParam?        par    = null)
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
            ReadOnlySpan<byte> data         ,
            Type               type         ,
        out object?            result       ,
            BytesParam?        par    = null)
    {
        par ??= new();
        par.Type = type;
        return TryTo(data, out result, par);
    }

    private static int TryTo(
            ReadOnlySpan<byte> data  ,
        out object?            result,
            BytesParam         par   )
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
                                ReadOnlySpan<byte> data  ,
        [NotNullWhen(true)] out object?            result,
                                BytesParam         par   )
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
                        data       ,
                        out sbyte n,
                        par        ,
                        true       );
                    result = n;
                    return ret;
                }
            case byte:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data      ,
                        out byte n,
                        par       ,
                        false     );
                    result = n;
                    return ret;
                }
            case short:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data       ,
                        out short n,
                        par        ,
                        true       );
                    result = n;
                    return ret;
                }
            case ushort:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data        ,
                        out ushort n,
                        par         ,
                        false       );
                    result = n;
                    return ret;
                }
            case int:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data     ,
                        out int n,
                        par      ,
                        true     );
                    result = n;
                    return ret;
                }
            case uint:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data      ,
                        out uint n,
                        par       ,
                        false     );
                    result = n;
                    return ret;
                }
            case long:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data      ,
                        out long n,
                        par       ,
                        true      );
                    result = n;
                    return ret;
                }
            case ulong:
                {
                    ret = TryReadIBinaryIntegerSpan(
                        data       ,
                        out ulong n,
                        par        ,
                        false      );
                    result = n;
                    return ret;
                }
            default:
                return -1;
        };
    }

    private static int TryReadBinaryObjectAttribute(
            ReadOnlySpan<byte> data  ,
        out object?            result,
            BytesParam         par   )
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
            var d = data;
            int sizeLimit = -1;
            if (m.Attrib.Size >= 0)
            {
                if (m.Attrib.Size > d.Length)
                    return -1;
                d = d[..m.Attrib.Size];
                sizeLimit = m.Attrib.Size;
            }

            int rb = TryTo(d, out var res, m.CreatePar(objPar));
            if (rb < 0)
                return -1;

            m.SetValue(result, res);

            rb = Math.Max(rb, sizeLimit);
            data = data[rb..];
            totalReaded += rb;
        }

        return totalReaded;
    }

    private static int TryReadIBinaryInteger(
            ReadOnlySpan<byte> data  ,
        out object?            result,
            BytesParam par           )
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

        // use reflection to call wrappers for
        // IBinaryInteger.TryReadLittleEndian or
        // IBinaryInteger.TryReadBigEndian with the type parameter
        var parm = new object[]
        {
            new SizedPointer<byte>(data), isUnsigned, null!
        };

        var res = (bool)typeof(Bytes).GetMethod(
                mname,
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;

        result = parm[2];
        res = res && result is not null;
        return res ? data.Length : -1;
    }

    private static int TryReadIBinaryObject(
            ReadOnlySpan<byte> data  ,
        out object?            result,
            BytesParam         par   )
    {
        result = null;

        // check whether the type implements the IBinaryObject interface
        if (!par.Type.GetInterfaces().Any(
            p => p.FullName is not null &&
                 p.FullName.Contains("Bny.RawBytes.IBinaryObject")))
            return -1;

        // use reflection to call the generic wrapper
        var parm = new object[]
        {
            new SizedPointer<byte>(data), null!, par.Endianness
        };

        var ret = (int)typeof(Bytes).GetMethod(
                nameof(TryReadIBinaryObjectWrapper),
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;
        result = parm[1];
        ret = result is null && ret >= 0 ? -1 : ret;
        return ret;
    }

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
            nameof(type)                                   );
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
    public static bool TryTo<T>(Stream      data         ,
        [NotNullWhen(true)] out T?          result       ,
                                BytesParam? par    = null)
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
    public static bool TryTo(   Stream      data         ,
                                Type        type         ,
        [NotNullWhen(true)] out object?     result       ,
                                BytesParam? par    = null)
    {
        par ??= new();
        par.Type = type;
        return TryTo(data, out result, par);
    }

    private static bool TryTo(  Stream     data  ,
        [NotNullWhen(true)] out object?    result,
                                BytesParam par   )
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
                                Stream     data  ,
        [NotNullWhen(true)] out object?    result,
                                BytesParam par   )
    {
        if (par.Type == typeof(string))
        {
            result = par.GetString(data);
            return result is not null;
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
                        data       ,
                        out sbyte n,
                        par        ,
                        true       );
                    result = n;
                    return ret;
                }
            case byte:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data      ,
                        out byte n,
                        par       ,
                        false     );
                    result = n;
                    return ret;
                }
            case short:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data       ,
                        out short n,
                        par        ,
                        true       );
                    result = n;
                    return ret;
                }
            case ushort:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data        ,
                        out ushort n,
                        par         ,
                        false       );
                    result = n;
                    return ret;
                }
            case int:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data     ,
                        out int n,
                        par      ,
                        true     );
                    result = n;
                    return ret;
                }
            case uint:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data      ,
                        out uint n,
                        par       ,
                        false     );
                    result = n;
                    return ret;
                }
            case long:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data      ,
                        out long n,
                        par       ,
                        true      );
                    result = n;
                    return ret;
                }
            case ulong:
                {
                    ret = TryReadIBinaryIntegerStream(
                        data       ,
                        out ulong n,
                        par        ,
                        false      );
                    result = n;
                    return ret;
                }
            default:
                return false;
        };
    }

    private static bool TryReadBinaryObjectAttribute(
                                Stream     data  ,
        [NotNullWhen(true)] out object?    result,
                                BytesParam par   )
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
            if (m.Attrib.Size >= 0)
            {
                Span<byte> buffer = new byte[m.Attrib.Size];
                if (data.Read(buffer) != buffer.Length)
                    return false;
                if (TryTo(buffer, out res, m.CreatePar(objPar)) < 0)
                    return false;
            }
            else
            {
                if (!TryTo(data, out res, m.CreatePar(objPar)))
                    return false;
            }

            m.SetValue(result, res);
        }
        return true;
    }

    private static bool TryReadIBinaryObject(
                                Stream     data  ,
        [NotNullWhen(true)] out object?    result,
                                BytesParam par   )
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
                                Stream     data  ,
        [NotNullWhen(true)] out object?    result,
                                BytesParam par   )
    {
        var buffer = new byte[128];
        using MemoryStream ms = new();

        int len;
        do ms.Write(buffer, 0, len = data.Read(buffer));
        while (len == buffer.Length);

        return TryReadIBinaryInteger(
            ms.GetBuffer().AsSpan()[..(int)ms.Position],
            out result, par                            ) == ms.Position;
    }

    // Wrappers for IBinaryInteger TryRead methods with
    // SizedPointer as parameter instead of Span
    private static bool TryReadIBinaryIntegerLEWrapper<T>(
            SizedPointer<byte> ptr       ,
            bool               isUnsigned,
        out T result                     ) where T : IBinaryInteger<T>
        => T.TryReadLittleEndian(ptr, isUnsigned, out result);

    private static bool TryReadIBinaryIntegerBEWrapper<T>(
            SizedPointer<byte> ptr       ,
            bool               isUnsigned,
        out T                  result    ) where T : IBinaryInteger<T>
        => T.TryReadBigEndian(ptr, isUnsigned, out result);

    // Wrappers for IBinaryObject TryRead methods
    private static int TryReadIBinaryObjectWrapper<T>(
            SizedPointer<byte> ptr       ,
        out T?                 result    ,
            Endianness         endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(ptr, out result, endianness);

    private static bool TryReadIBinaryObjectSWrapper<T>(
            Stream     str       ,
        out T?         result    ,
            Endianness endianness) where T : IBinaryObject<T>
        => T.TryReadFromBinary(str, out result, endianness);

    private static int TryReadIBinaryIntegerSpan<T>(
                                ReadOnlySpan<byte> span  ,
        [NotNullWhen(true)] out T?                 result,
                                BytesParam         par   ,
                                bool               signed)
        where T : IBinaryInteger<T>
    {
        int size = Marshal.SizeOf<T>();
        span = span[..size];

        if (par.GetEndianness(DefaultEndianness) == Endianness.Little
                ? T.TryReadLittleEndian(span, !signed, out result)
                : T.TryReadBigEndian   (span, !signed, out result))
            return size;
        return -1;
    }

    private static bool TryReadIBinaryIntegerStream<T>(
                                Stream     str    ,
        [NotNullWhen(true)] out T?         result ,
                                BytesParam par    ,
                                bool       signed )
        where T : IBinaryInteger<T>
    {
        int size = Marshal.SizeOf<T>();
        Span<byte> buffer = stackalloc byte[size];
        str.Read(buffer);

        return TryReadIBinaryIntegerSpan(
            buffer    ,
            out result,
            par       ,
            signed    ) >= 0;
    }

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>number of written bytes</returns>
    /// <exception cref="ArgumentException">
    /// thrown for unsupported types
    /// </exception>
    public static int From<T>(
        T?          value        ,
        Span<byte>  result       ,
        BytesParam? par    = null)
    {
        var ret = TryFrom(value, result, typeof(T), par);
        return ret < 0
            ? throw new ArgumentException(
                "Cannot convert to this value type",
                nameof(value)                      )
            : ret;
    }

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>number of written bytes</returns>
    /// <exception cref="ArgumentException">
    /// thrown for unsupported types
    /// </exception>
    public static int From(
        object?     value        ,
        Span<byte>  result       ,
        BytesParam? par    = null)
    {
        var ret = TryFrom(value, result, par);
        return ret < 0
            ? throw new ArgumentException(
                "Cannot convert to this value type",
                nameof(value)                      )
            : ret;
    }

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>number of written bytes, -1 on error</returns>
    public static int TryFrom<T>(
        T?          value        ,
        Span<byte>  result       ,
        BytesParam? par    = null)
        => TryFrom(value, result, typeof(T), par);

    /// <summary>
    /// Converts the value into byte array
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="result">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>number of written bytes, -1 on error</returns>
    public static int TryFrom(
        object?     value        ,
        Span<byte>  result       ,
        BytesParam? par    = null)
        => TryFrom(value, result, value?.GetType()!, par);

    private static int TryFrom(
        object?     value ,
        Span<byte>  result,
        Type        type  ,
        BytesParam? par   )
    {
        par ??= new();
        par.Type = type;
        return TryFrom_(value, result, par);
    }

    private static int TryFrom_(
        object?    value ,
        Span<byte> result,
        BytesParam par   )
    {
        if (value is null)
            return -1;
        int len;
        if ((len = TryWriteBasicSpan(value, result, par)) >= 0)
            return len;
        if ((len = TryWriteBinaryAttribute(value, result, par)) >= 0)
            return len;
        if ((len = TryWriteIBinaryObjectWrite(value, result, par)) >= 0)
            return len;
        if ((len = TryWriteIBinaryInteger(value, result, par)) >= 0)
            return len;
        return -1;
    }

    private static int TryWriteBasicSpan(
        object?    value ,
        Span<byte> result,
        BytesParam par   )
    {
        if (value is not string str)
            return -1;

        return par.GetBytes(str, result);
    }

    private static int TryWriteBinaryAttribute(
        object     value ,
        Span<byte> result,
        BytesParam par   )
    {
        if (!TryExtractAttribute(par, out _, out var members, out var objPar))
            return -1;

        int bytesWritten = 0;
        foreach (var m in members)
        {
            var wb = TryFrom_(
                m.GetValue(value)! ,
                result             ,
                m.CreatePar(objPar));

            if (wb < 0)
                return -1;

            result = result[wb..];
            bytesWritten += wb;
        }

        return bytesWritten;
    }

    private static int TryWriteIBinaryObjectWrite(
        object     value ,
        Span<byte> result,
        BytesParam par   )
    {
        if (value is not IBinaryObjectWrite bow)
            return -1;
        return bow.TryWriteToBinary(result, par.Endianness);
    }

    private static int TryWriteIBinaryInteger(
        object     data  ,
        Span<byte> result,
        BytesParam par   )
    {
        string mname = par.GetEndianness() == Endianness.Little
            ? nameof(TryWriteIBinaryIntegerLEWrapper)
            : nameof(TryWriteIBinaryIntegerBEWrapper);

        var bi = par.Type.GetInterfaces().FirstOrDefault(
            p => p.FullName is not null &&
                 p.FullName.Contains("System.Numerics.IBinaryInteger"));

        if (bi is null)
            return -1;

        try
        {
            int byteCount = (int)bi.GetMethod(
                    nameof(IBinaryInteger<int>.GetByteCount),
                    Array.Empty<Type>()                     )!
                .Invoke(data, Array.Empty<object>())!;

            if (result.Length < byteCount)
                return -1;

            var parm = new object[]
            {
                data, new SizedPointer<byte>(result), null!
            };

            var res = (bool)typeof(Bytes).GetMethod(
                    mname                                       ,
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;

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
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <exception cref="ArgumentException">
    /// thrown for unsupported types
    /// </exception>
    public static void From<T>(
        T?          value        ,
        Stream      output       ,
        BytesParam? par    = null)
    {
        if (!TryFrom(value, output, typeof(T), par))
            throw new ArgumentException(
                "Cannot convert to this value type from stream",
                nameof(value)                                  );
    }

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <param name="value">vylue to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <exception cref="ArgumentException">
    /// thrown for unsupported types
    /// </exception>
    public static void From(
        object?     value ,
        Stream      output,
        BytesParam? par    = null)
    {
        if (!TryFrom(value, output, value?.GetType()!, par))
            throw new ArgumentException(
                "Cannot convert to this value type from stream",
                nameof(value)                                  );
    }

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <typeparam name="T">Type of the value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryFrom<T>(
        T?          value        ,
        Stream      output       ,
        BytesParam? par    = null)
        => TryFrom(value, output, typeof(T), par);

    /// <summary>
    /// Converts the value into bytes and writes it to a stream
    /// </summary>
    /// <param name="value">value to convert</param>
    /// <param name="output">Where the bytes will be written</param>
    /// <param name="par">
    /// the conversion parameters, null creates default conversion parameters
    /// </param>
    /// <returns>true on success, otherwise false</returns>
    public static bool TryFrom(
        object?     value        ,
        Stream      output       ,
        BytesParam? par    = null)
        => TryFrom(value, output, value?.GetType()!, par);

    private static bool TryFrom(
        object?     value ,
        Stream      output,
        Type        type  ,
        BytesParam? par   )
    {
        par ??= new();
        par.Type = type;
        return TryFrom_(value, output, par);
    }

    private static bool TryFrom_(object? value, Stream output, BytesParam par)
    {
        if (value is null)
            return false;
        if (TryWriteBasicStream(value, output, par))
            return true;
        if (TryWriteBinaryObjectAttribute(value, output, par))
            return true;
        if (TryWriteIBinaryObjectWrite(value, output, par))
            return true;
        if (TryWriteIBinaryInteger(value, output, par))
            return true;
        return false;
    }

    private static bool TryWriteBasicStream(
        object?    value ,
        Stream     output,
        BytesParam par   )
    {
        if (value is not string str)
            return false;

        return par.GetBytes(str, output);
    }

    private static bool TryWriteBinaryObjectAttribute(
        object     value ,
        Stream     output,
        BytesParam par   )
    {
        if (!TryExtractAttribute(par, out _, out var members, out var objPar))
            return false;

        foreach (var m in members)
        {
            if (!TryFrom_(m.GetValue(value)!, output, m.CreatePar(objPar)))
                return false;
        }
        return true;
    }

    private static bool TryWriteIBinaryObjectWrite(
        object     value ,
        Stream     output,
        BytesParam par   )
    {
        if (value is not IBinaryObjectWrite bow)
            return false;

        var size = bow.WriteSize;
        Span<byte> buffer = new byte[size];
        bow.TryWriteToBinary(buffer, par.Endianness);
        output.Write(buffer);
        return true;
    }

    private static bool TryWriteIBinaryInteger(
        object     value ,
        Stream     output,
        BytesParam par   )
    {
        var interfaces = par.Type.GetInterfaces();

        var intf = interfaces.FirstOrDefault(
            p => p.FullName is not null &&
                 p.FullName.Contains("System.Numerics.IBinaryInteger"));

        if (intf is null)
            return false;

        int size = (int)intf.GetMethod(
                nameof(IBinaryInteger<int>.GetByteCount))!
            .Invoke(value, Array.Empty<object>())!;

        Span<byte> buffer = new byte[size];
        if (TryFrom_(value, buffer, par) < 0)
            return false;
        output.Write(buffer);
        return true;
    }

    private static bool TryWriteIBinaryIntegerLEWrapper<T>(
            T                  value       ,
            SizedPointer<byte> ptr         ,
        out int                bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteLittleEndian(ptr, out bytesWritten);

    private static bool TryWriteIBinaryIntegerBEWrapper<T>(
            T                  value       , 
            SizedPointer<byte> ptr         ,
        out int                bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteBigEndian(ptr, out bytesWritten);

    private static bool TryExtractAttribute(
                                BytesParam                      par      ,
        [NotNullWhen(true)] out BinaryObjectAttribute?          attrib   ,
                            out Span<BinaryMemberAttributeInfo> members  ,
        [NotNullWhen(true)] out BytesParam?                     typeParam)
    {
        typeParam = null;
        members = Array.Empty<BinaryMemberAttributeInfo>().AsSpan();

        attrib = par.Type.GetCustomAttribute<BinaryObjectAttribute>();
        if (attrib is null)
            return false;

        const BindingFlags AllBindingFlags =
            BindingFlags.Public    |
            BindingFlags.NonPublic |
            BindingFlags.Instance  |
            BindingFlags.Static    ;

        members = par.Type.GetFields(AllBindingFlags)
            .Select(p => new BinaryMemberAttributeInfo(p))
            .Concat(par.Type.GetProperties(AllBindingFlags)
            .Select(p => new BinaryMemberAttributeInfo(p)))
            .Where(p => p.Attrib is not null).OrderBy(p => p.Attrib.Order)
            .ToArray().AsSpan();

        typeParam = par with
        {
            Endianness = attrib.Endianness == Endianness.Default
                ? par.GetEndianness(DefaultEndianness)
                : attrib.Endianness,
        };

        return true;
    }
}
