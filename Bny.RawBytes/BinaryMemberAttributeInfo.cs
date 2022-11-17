using System.Reflection;

namespace Bny.RawBytes;

internal class BinaryMemberAttributeInfo
{
    public BinaryMemberAttribute Attrib { get; init; }
    public Func<object?, object?> GetValue { get; init; }
    public Action<object?, object?> SetValue { get; init; }
    public Type MemberType { get; init; }

    public BinaryMemberAttributeInfo(FieldInfo field)
    {
        Attrib = field.GetCustomAttribute<BinaryMemberAttribute>()!;
        GetValue = field.GetValue;
        SetValue = field.SetValue;
        MemberType = field.FieldType;
    }
    public BinaryMemberAttributeInfo(PropertyInfo property)
    {
        Attrib = property.GetCustomAttribute<BinaryMemberAttribute>()!;
        GetValue = property.GetValue;
        SetValue = property.SetValue;
        MemberType = property.PropertyType;
    }
}
