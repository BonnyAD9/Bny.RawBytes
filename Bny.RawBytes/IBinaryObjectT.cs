using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bny.RawBytes;

/// <summary>
/// Generic methods for converting the type to and from binary data
/// </summary>
public interface IBinaryObject<TSelf> : IBinaryObject where TSelf : IBinaryObject<TSelf>
{
    /// <summary>
    /// Tries to read the object from binary data
    /// </summary>
    /// <typeparam name="T">Type of the data</typeparam>
    /// <param name="data">binary data to read from</param>
    /// <param name="result">the resulting object, shouldn't be null of this returns true</param>
    /// <param name="endianness">prefered endianness of the conversion</param>
    /// <returns>True on success, otherwise false</returns>
    public static abstract bool TryFromBinary(ReadOnlySpan<byte> data, [NotNullWhen(true)] out TSelf? result, Endianness endianness = Endianness.Default);

    static bool IBinaryObject.TryFromBinary(ReadOnlySpan<byte> data, [NotNullWhen(true)] out object? result, Endianness endianness)
    {
        var ret = TSelf.TryFromBinary(data, out TSelf? res, endianness);
        result = res;
        return ret;
    }
}
