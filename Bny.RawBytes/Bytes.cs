using System.Diagnostics.CodeAnalysis;
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
    public static T To<T>(ReadOnlySpan<byte> data, Endianness endianness = Endianness.Default, bool? signed = null, int index = 0) where T : new()
        => (T)To(data, typeof(T), endianness, signed, index);

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
    public static object To(ReadOnlySpan<byte> data, Type type, Endianness endianness = Endianness.Default, bool? signed = null, int index = 0)
    {
        object res = Activator.CreateInstance(type)!;
        if (TryReadIBInaryInteger(data, ref res, type, index, endianness, signed))
            return res;
        throw new ArgumentException("Cannot convert to this value type", nameof(type));
    }

    private static bool TryReadIBInaryInteger(ReadOnlySpan<byte> data, ref object result, Type type, int index = 0, Endianness endianness = Endianness.Default, bool? signed = false)
    {
        string mname = endianness switch
        {
            Endianness.Big => "ReadBigEndian",
            Endianness.Little => "ReadLittleEndian",
            Endianness.Default => IsDefaultLE ? "ReadLittleEndian" : "ReadBigEndian",
            _ => throw new ArgumentException("Invalid endianness value")
        };

        var interfaces = type.GetInterfaces();
        var bi = interfaces.FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (bi is null)
            return false;

        bool isUnsigned = signed.HasValue ? !signed.Value : !interfaces.Any(p => p.FullName is not null && p.FullName.Contains("System.Numerics.ISignedNumber"));

        try
        {
            var arr = data.ToArray();
            result = bi.GetMethod(mname, new Type[] { typeof(byte[]), typeof(int), typeof(bool) })!.Invoke(result, new object[] { arr, index, isUnsigned })!;
        }
        catch
        {
            return false;
        }
        return true;
    }
}
