﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;

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
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="endianness"></param>
    /// <param name="signed"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static T To<T>(ReadOnlySpan<byte> data, Endianness endianness = Endianness.Default, bool? signed = null) where T : new()
        => (T)To(data, typeof(T), endianness, signed);

    /// <summary>
    /// Converts byte array to the given type
    /// </summary>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <param name="endianness"></param>
    /// <param name="signed"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static object To(ReadOnlySpan<byte> data, Type type, Endianness endianness = Endianness.Default, bool? signed = null)
    {
        object res = Activator.CreateInstance(type)!;
        if (TryReadIBinaryInteger(data, ref res, type, endianness, signed))
            return res;
        throw new ArgumentException("Cannot convert to this value type", nameof(type));
    }

    private static bool TryReadIBinaryInteger(ReadOnlySpan<byte> data, ref object result, Type type, Endianness endianness = Endianness.Default, bool? signed = false)
    {
        string mname = endianness switch
        {
            Endianness.Big => "ReadBigEndian",
            Endianness.Little => "ReadLittleEndian",
            Endianness.Default => IsDefaultLE ? "ReadLittleEndian" : "ReadBigEndian",
            _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
        };

        var interfaces = type.GetInterfaces();
        var bi = interfaces.FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (bi is null)
            return false;

        bool isUnsigned = signed.HasValue ? !signed.Value : !interfaces.Any(p => p.FullName is not null && p.FullName.Contains("System.Numerics.ISignedNumber"));

        try
        {
            var arr = data.ToArray();
            result = bi.GetMethod(mname, new Type[] { typeof(byte[]), typeof(int), typeof(bool) })!.Invoke(result, new object[] { arr, 0, isUnsigned })!;
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <param name="endianness"></param>
    /// <returns></returns>
    public static int From<T>(T value, Span<byte> result, Endianness endianness = Endianness.Default)
        => From(value!, result, typeof(T), endianness);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="endianness"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int From(object value, Span<byte> result, Type type, Endianness endianness = Endianness.Default)
    {
        int len = TryWriteIBinaryInteger(value, result, type, endianness);
        if (len != 0)
            return len;
        throw new ArgumentException("Cannot convert from this value type", nameof(value));
    }

    private static int TryWriteIBinaryInteger(object data, Span<byte> result, Type type, Endianness endianness = Endianness.Default)
    {
        string mname = endianness switch
        {
            Endianness.Big => "WriteBigEndian",
            Endianness.Little => "WriteLittleEndian",
            Endianness.Default => IsDefaultLE ? "WriteLittleEndian" : "WriteBigEndian",
            _ => throw new ArgumentException("Invalid endianness value", nameof(endianness)),
        };

        var interfaces = type.GetInterfaces();
        var bi = interfaces.FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (bi is null)
            return 0;

        try
        {
            int byteCount = (int)bi.GetMethod("GetByteCount", Array.Empty<Type>())!.Invoke(data, Array.Empty<object>())!;
            if (result.Length < byteCount)
                return 0;

            byte[] arr = new byte[byteCount];
            int r = (int)bi.GetMethod(mname, new Type[] { typeof(byte[]) })!.Invoke(data, new object[] { arr })!;

            arr.AsSpan()[..r].CopyTo(result);
            return r;
        }
        catch
        {
            return 0;
        }
    }
}
