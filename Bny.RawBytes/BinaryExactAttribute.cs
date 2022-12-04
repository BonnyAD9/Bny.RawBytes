using System.Runtime.CompilerServices;

namespace Bny.RawBytes;

/// <summary>
/// Checks whether the exact data is present
/// </summary>
[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field    ,
    AllowMultiple = true      ,
    Inherited = false         )]
public class BinaryExactAttribute : BinaryAttribute
{
    /// <summary>
    /// Data to be readed (will be encoded to byte array and than checked)
    /// </summary>
    public string Data => _data;
    private readonly string _data;

    /// <summary>
    /// Encoding of the Data string
    /// </summary>
    public string DataEncoding => _dataEncoding;
    private readonly string _dataEncoding;

    /// <summary>
    /// Creates attribute that checks whether the given data is exactly
    /// present in the data
    /// </summary>
    /// <param name="data">
    /// data to be checked (will be converted to bytes before checking)
    /// </param>
    /// <param name="dataEncoding">
    /// Encoding in which to convert data to bytes
    /// </param>
    /// <param name="order">
    /// Order of the attribute (line number by default)
    /// </param>
    public BinaryExactAttribute(string data = "", string dataEncoding = "utf-8", [CallerLineNumber] int order = 0) : base(order)
    {
        _data = data;
        _dataEncoding = dataEncoding;
    }
}
