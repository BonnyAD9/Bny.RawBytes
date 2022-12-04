using Bny.General.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bny.RawBytes;

/// <summary>
/// Contains functions for converting from and to byte array
/// </summary>
public static partial class Bytes
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

    private static bool TryExtractAttribute(
                                BytesParam                par      ,
        [NotNullWhen(true)] out BinaryObjectAttribute?    attrib   ,
                            out Span<BinaryAttributeInfo> members  ,
        [NotNullWhen(true)] out BytesParam?               typeParam)
    {
        typeParam = null;
        members = Array.Empty<BinaryAttributeInfo>().AsSpan();

        attrib = par.Type.GetCustomAttribute<BinaryObjectAttribute>();
        if (attrib is null)
            return false;

        const BindingFlags AllBindingFlags =
            BindingFlags.Public    |
            BindingFlags.NonPublic |
            BindingFlags.Instance  |
            BindingFlags.Static    ;

        members = (from b in
                   (from f in par.Type.GetFields(AllBindingFlags)
                    from a in f.GetCustomAttributes<BinaryAttribute>()
                    select new BinaryAttributeInfo(f, a))
                   .Concat(
                    from p in par.Type.GetProperties(AllBindingFlags)
                    from a in p.GetCustomAttributes<BinaryAttribute>()
                    select new BinaryAttributeInfo(p, a))
                   orderby b.Attrib.Order
                   select b)
                  .ToArray();

        typeParam = par with
        {
            Endianness = attrib.Endianness == Endianness.Default
                ? par.GetEndianness(DefaultEndianness)
                : attrib.Endianness,
        };

        return true;
    }
}
