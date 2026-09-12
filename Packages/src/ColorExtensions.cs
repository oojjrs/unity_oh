using UnityEngine;

public static class ColorExtensions
{
    public static string ToHtmlStringRgb(this Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    public static string ToHtmlStringRgba(this Color color)
    {
        return ColorUtility.ToHtmlStringRGBA(color);
    }

    public static void ToHsv(this Color color, out float h, out float s, out float v)
    {
        Color.RGBToHSV(color, out h, out s, out v);
    }

    public static string ToRichText(this Color color, string text)
    {
        return $"<color=#{color.ToHtmlStringRgba()}>{text}</color>";
    }

    public static Color WithAlpha(this Color color, float alpha)
    {
        return new(color.r, color.g, color.b, alpha);
    }
}
