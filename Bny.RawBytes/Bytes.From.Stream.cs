using System.Numerics;

namespace Bny.RawBytes;

public static partial class Bytes
{

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
        T? value,
        Stream output,
        BytesParam? par = null)
    {
        if (!TryFrom(value, output, typeof(T), par))
            throw new ArgumentException(
                "Cannot convert to this value type from stream",
                nameof(value));
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
        object? value,
        Stream output,
        BytesParam? par = null)
    {
        if (!TryFrom(value, output, value?.GetType()!, par))
            throw new ArgumentException(
                "Cannot convert to this value type from stream",
                nameof(value));
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
        T? value,
        Stream output,
        BytesParam? par = null)
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
        object? value,
        Stream output,
        BytesParam? par = null)
        => TryFrom(value, output, value?.GetType()!, par);

    private static bool TryFrom(
        object? value,
        Stream output,
        Type type,
        BytesParam? par)
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
        object? value,
        Stream output,
        BytesParam par)
    {
        if (value is string str)
            return par.GetBytes(str, output);

        if (!par.Type.IsEnum)
            return false;

        var newPar = par with { Type = par.Type.GetEnumUnderlyingType() };

        return
            TryFrom_(Convert.ChangeType(value, newPar.Type), output, newPar);
    }

    private static bool TryWriteBinaryObjectAttribute(
        object value,
        Stream output,
        BytesParam par)
    {
        if (!TryExtractAttribute(par, out _, out var members, out var objPar))
            return false;

        foreach (var m in members)
        {
            switch (m.Attrib)
            {
                case BinaryMemberAttribute bma:
                    if (bma.Size != -1)
                    {
                        if (bma.Size < 0)
                            return false;

                        MaxLengthStream mls = new(output, bma.Size, fakeLengths: bma.TrimLargeData);
                        if (!TryFrom_(m.GetValue(value)!, mls, m.CreatePar(bma, objPar)))
                            return false;

                        if (mls.CurPos != bma.Size)
                            output.Write(new byte[bma.Size - mls.CurPos]);
                        break;
                    }

                    if (!TryFrom_(m.GetValue(value)!, output, m.CreatePar(bma, objPar)))
                        return false;
                    break;
                case BinaryPaddingAttribute bpa:
                    if (bpa.Size < 0)
                        return false;
                    output.Write(new byte[bpa.Size]); // write bpa.Size zeros
                    break;
                case BinaryExactAttribute bea:
                    {
                        var encoding =
                            BinaryEncoding.TryGet(bea.DataEncoding);

                        if (encoding is null)
                            return false;

                        output.Write(encoding.GetBytes(bea.Data));
                        break;
                    }
                case CustomBinaryAttribute cba:
                    if (!cba.WriteToStream(
                        m.GetValue(value),
                        output,
                        objPar with { Type = m.MemberType }
                    ))
                        return false;
                    break;
                default:
                    return false;
            }
        }
        return true;
    }

    private static bool TryWriteIBinaryObjectWrite(
        object value,
        Stream output,
        BytesParam par)
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
        object value,
        Stream output,
        BytesParam par)
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
}
