using System;

public static class StringExtensions
{
    public static T ToEnum<T>(this string s) where T : Enum
    {
        return ToEnum(s, default(T));
    }

    public static T ToEnum<T>(this string s, T defValue) where T : Enum
    {
        if (string.IsNullOrWhiteSpace(s))
            return defValue;
        else
            return (T)Enum.Parse(typeof(T), s);
    }
}
