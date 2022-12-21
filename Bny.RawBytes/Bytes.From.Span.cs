using Bny.General.Memory;
using System.Numerics;
using System.Reflection;

namespace Bny.RawBytes;

public static partial class Bytes
{
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
        T? value,
        Span<byte> result,
        BytesParam? par = null)
    {
        var ret = TryFrom(value, result, typeof(T), par);
        return ret < 0
            ? throw new ArgumentException(
                "Cannot convert to this value type",
                nameof(value))
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
        object? value,
        Span<byte> result,
        BytesParam? par = null)
    {
        var ret = TryFrom(value, result, par);
        return ret < 0
            ? throw new ArgumentException(
                "Cannot convert to this value type",
                nameof(value))
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
        T? value,
        Span<byte> result,
        BytesParam? par = null)
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
        object? value,
        Span<byte> result,
        BytesParam? par = null)
        => TryFrom(value, result, value?.GetType()!, par);

    private static int TryFrom(
        object? value,
        Span<byte> result,
        Type type,
        BytesParam? par)
    {
        par ??= new();
        par.Type = type;
        return TryFrom_(value, result, par);
    }

    private static int TryFrom_(
        object? value,
        Span<byte> result,
        BytesParam par)
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
        object? value,
        Span<byte> result,
        BytesParam par)
    {
        if (value is string str)
            return par.GetBytes(str, result);

        if (!par.Type.IsEnum)
            return -1;

        var newPar = par with { Type = par.Type.GetEnumUnderlyingType() };

        return
            TryFrom_(Convert.ChangeType(value, newPar.Type), result, newPar);
    }

    private static int TryWriteBinaryAttribute(
        object value,
        Span<byte> result,
        BytesParam par)
    {
        if (!TryExtractAttribute(par, out _, out var members, out var objPar))
            return -1;

        int bytesWritten = 0;
        foreach (var m in members)
        {
            int wb; // written bytes in this iteration
            switch (m.Attrib)
            {
                case BinaryMemberAttribute bma:
                    if (bma.Size != -1)
                    {
                        if (bma.Size > result.Length)
                            return -1;

                        var resultRange = result;
                        if (bma.Size < result.Length)
                            resultRange = result[..bma.Size];

                        wb = TryFrom_(
                            m.GetValue(value)!      ,
                            resultRange             ,
                            m.CreatePar(bma, objPar));

                        if (wb < 0)
                            return -1;

                        if (wb < bma.Size)
                        {
                            resultRange[wb..].Clear();
                            wb = bma.Size;
                        }
                        break;
                    }

                    wb = TryFrom_(
                        m.GetValue(value)!,
                        result,
                        m.CreatePar(bma, objPar));

                    if (wb < 0)
                        return -1;
                    break;
                case BinaryPaddingAttribute bpa:
                    wb = bpa.Size;
                    if (result.Length < wb)
                        return -1;
                    result[..wb].Clear();
                    break;
                case BinaryExactAttribute bea:
                    {
                        var encoding =
                            BinaryEncoding.TryGet(bea.DataEncoding);

                        if (encoding is null)
                            return -1;

                        ReadOnlySpan<byte> match =
                            encoding.GetBytes(bea.Data);
                        wb = match.Length;

                        if (result.Length < wb)
                            return -1;

                        match.CopyTo(result[..wb]);
                        break;
                    }
                default:
                    return -1;
            }

            result = result[wb..];
            bytesWritten += wb;
        }

        return bytesWritten;
    }

    private static int TryWriteIBinaryObjectWrite(
        object value,
        Span<byte> result,
        BytesParam par)
    {
        if (value is not IBinaryObjectWrite bow)
            return -1;
        return bow.TryWriteToBinary(result, par.Endianness);
    }

    private static unsafe int TryWriteIBinaryInteger(
        object data,
        Span<byte> result,
        BytesParam par)
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
                    Array.Empty<Type>())!
                .Invoke(data, Array.Empty<object>())!;

            if (result.Length < byteCount)
                return -1;

            object[] parm;
            bool res;

            fixed (byte* ptr = result) // ensure that the memory is fixed
            {
                parm = new object[]
                {
                    data, new SpanWrapper<byte>(result), null!
                };

                res = (bool)typeof(Bytes).GetMethod(
                        mname,
                        BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(par.Type)!.Invoke(null, parm)!;
            }

            return res ? (int)parm[2] : -1;
        }
        catch
        {
            return -1;
        }
    }

    private static bool TryWriteIBinaryIntegerLEWrapper<T>(
            T value,
            SpanWrapper<byte> ptr,
        out int bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteLittleEndian(ptr, out bytesWritten);

    private static bool TryWriteIBinaryIntegerBEWrapper<T>(
            T value,
            SpanWrapper<byte> ptr,
        out int bytesWritten) where T : IBinaryInteger<T>
        => value.TryWriteBigEndian(ptr, out bytesWritten);
}
