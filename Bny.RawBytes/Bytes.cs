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
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T To<T>(byte[] data, Endianness endianness = Endianness.Default, bool? signed = null, int index = 0) where T : new()
    {
        var type = typeof(T);
        T res = new();
        if (TryReadIBInaryInteger(data, ref res, index, endianness, signed.HasValue ? signed.Value : res is sbyte or short or int or long or BigInteger))
            return res;
        throw new ArgumentException("Cannot convert to this value type", nameof(T));
    }

    private static bool TryReadIBInaryInteger<T>(byte[] data, ref T result, int index = 0, Endianness endianness = Endianness.Default, bool signed = false) where T : new()
    {
        string mname = endianness switch
        {
            Endianness.Big => "ReadBigEndian",
            Endianness.Little => "ReadLittleEndian",
            Endianness.Default => IsDefaultLE ? "ReadLittleEndian" : "ReadBigEndian",
            _ => throw new ArgumentException("Invalid endianness value")
        };

        var type = typeof(T);

        var bi = type.GetInterfaces().FirstOrDefault(p => p.FullName is not null && p.FullName.Contains("System.Numerics.IBinaryInteger"));
        if (bi is null)
            return false;

        try
        {
            result = (T)bi.GetMethod(mname, new Type[] { typeof(byte[]), typeof(int), typeof(bool) })!.Invoke(result, new object[] { data, index, !signed })!;
        }
        catch
        {
            return false;
        }
        return true;
    }
}
