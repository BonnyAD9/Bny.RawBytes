using System.Diagnostics.CodeAnalysis;

namespace Bny.RawBytes;

/// <summary>
/// Derive from this class to create custom attribute for binary data
/// </summary>
public abstract class CustomBinaryAttribute : BinaryAttribute
{
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="order">
    /// Order of the attributes, should be set with the [CallerLineNumber]
    /// attribute
    /// </param>
    public CustomBinaryAttribute(int order) : base(order) { }

    /// <summary>
    /// Reads the object from span
    /// </summary>
    /// <param name="span">Span to read from</param>
    /// <param name="obj">
    /// Resulting object. Can be null only if the return value is negative.
    /// </param>
    /// <param name="param">Conversion parameters</param>
    /// <returns>
    /// Number of bytes readed from the span on success, otherwise negative
    /// </returns>
    public abstract int ReadFromSpan(
            ReadOnlySpan<byte> span ,
        out object?            obj  ,
            BytesParam         param);

    /// <summary>
    /// Reads the object from stream
    /// </summary>
    /// <param name="stream">Stream to read from</param>
    /// <param name="obj">
    /// Resulting object. Can be null only if the return value is false
    /// </param>
    /// <param name="param">Conversion parameters</param>
    /// <returns>True on success, otherwise false</returns>
    public abstract bool ReadFromStream(
                                Stream     stream,
        [NotNullWhen(true)] out object?    obj   ,
                                BytesParam param );

    /// <summary>
    /// Writes the given object data to span
    /// </summary>
    /// <param name="obj">Object to write the data</param>
    /// <param name="output">Where to write the data</param>
    /// <param name="param">Conversion parameters</param>
    /// <returns>
    /// Number of bytes written on success, otherwise negative
    /// </returns>
    public abstract int WriteToSpan(
        object?    obj   ,
        Span<byte> output,
        BytesParam param );

    /// <summary>
    /// Writes the given object to the stream
    /// </summary>
    /// <param name="obj">Object to write to the stream</param>
    /// <param name="output">Stream to write to</param>
    /// <param name="param">Conversion parameters</param>
    /// <returns>True on success, otherwise false</returns>
    public abstract bool WriteToStream(
        object?    obj   ,
        Stream     output,
        BytesParam param );
}
