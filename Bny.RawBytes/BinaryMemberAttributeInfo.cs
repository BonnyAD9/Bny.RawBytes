﻿using System.Reflection;

namespace Bny.RawBytes;

internal class BinaryMemberAttributeInfo
{
    public BinaryMemberAttribute Attrib { get; init; }
    private Func<object?, object?> _getValue { get; init; }
    private Action<object?, object?> _setValue { get; init; }
    public Type MemberType { get; init; }

    public BinaryMemberAttributeInfo(FieldInfo field)
    {
        Attrib = field.GetCustomAttribute<BinaryMemberAttribute>()!;
        _getValue = field.GetValue;
        _setValue = field.SetValue;
        MemberType = field.FieldType;
    }
    public BinaryMemberAttributeInfo(PropertyInfo property)
    {
        Attrib = property.GetCustomAttribute<BinaryMemberAttribute>()!;
        _getValue = property.GetValue;
        _setValue = property.SetValue;
        MemberType = property.PropertyType;
    }

    public object? GetValue(object? instance) => _getValue(instance);
    public void SetValue(object? instance, object? value) => _setValue(instance, value);
}
