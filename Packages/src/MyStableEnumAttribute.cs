using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public sealed class MyStableEnumAttribute : PropertyAttribute
{
    public Type EnumType { get; }

    public MyStableEnumAttribute(Type enumType)
    {
        EnumType = enumType;
    }
}
