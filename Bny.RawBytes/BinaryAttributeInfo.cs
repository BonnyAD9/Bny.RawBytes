using System.Reflection;

namespace Bny.RawBytes;

internal class BinaryAttributeInfo
{
    public BinaryAttribute Attrib { get; init; }
    private readonly Func<object?, object?> _getValue;
    private readonly Action<object?, object?> _setValue;
    public Type MemberType { get; init; }

    public BinaryAttributeInfo(FieldInfo field, BinaryAttribute attribute)
    {
        Attrib = attribute;
        _getValue = field.GetValue;
        _setValue = field.SetValue;
        MemberType = field.FieldType;
    }
    public BinaryAttributeInfo(PropertyInfo property, BinaryAttribute attribute)
    {
        Attrib = attribute;
        _getValue = property.GetValue;
        _setValue = property.SetValue;
        MemberType = property.PropertyType;
    }

    public object? GetValue(object? instance) => _getValue(instance);
    public void SetValue(object? instance, object? value)
        => _setValue(instance, value);

    public BytesParam CreatePar(
        BinaryMemberAttribute attribute,
        BytesParam            par      ) => par with
    {
        Type = MemberType,
        Endianness = par.GetEndiannessTo(attribute.Endianness),
        Sign = attribute.Signed,
        Encoding = attribute.Encoding ?? par.Encoding,
        NullTerminated = attribute.NullTerminated,
    };
}
