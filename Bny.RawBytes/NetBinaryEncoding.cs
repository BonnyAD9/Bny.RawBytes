using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bny.RawBytes;

/// <summary>
/// Encoding from the .NET EncodingInfo
/// </summary>
public class NetBinaryEncoding : BinaryEncoding
{
    /// <summary>
    /// The encoding
    /// </summary>
    public EncodingInfo Encoding { get; init; }

    /// <inheritdoc/>
    public NetBinaryEncoding(EncodingInfo encodingInfo)
    {
        Encoding = encodingInfo;
        Init();
    }

    /// <inheritdoc/>
    public override string Name => Encoding.Name;

    /// <inheritdoc/>
    public override string GetString(ReadOnlySpan<byte> data)
        => Encoding.GetEncoding().GetString(data);

    /// <inheritdoc/>
    public override byte[] GetBytes(string str)
        => Encoding.GetEncoding().GetBytes(str);
}
