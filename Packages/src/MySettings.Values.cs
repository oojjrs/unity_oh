using System;
using System.Globalization;
using UnityEngine;

public static partial class MySettings
{
    private static class Values
    {
        public static string Format(bool value)
        {
            return value ? "true" : "false";
        }

        public static string Format(Color value)
        {
            return string.Join(",", Format(value.r), Format(value.g), Format(value.b), Format(value.a));
        }

        public static string Format(Color32 value)
        {
            return string.Join(",", Format(value.r), Format(value.g), Format(value.b), Format(value.a));
        }

        public static string Format(DateTime value)
        {
            return value.ToString("O", CultureInfo.InvariantCulture);
        }

        public static string Format(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        public static string Format(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        public static string Format(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Format(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Format(Quaternion value)
        {
            return string.Join(",", Format(value.x), Format(value.y), Format(value.z), Format(value.w));
        }

        public static string Format(string value)
        {
            return value;
        }

        public static string Format(TimeSpan value)
        {
            return value.ToString("c", CultureInfo.InvariantCulture);
        }

        public static string Format(uint value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Format(ulong value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Format(Vector2 value)
        {
            return string.Join(",", Format(value.x), Format(value.y));
        }

        public static string Format(Vector2Int value)
        {
            return string.Join(",", Format(value.x), Format(value.y));
        }

        public static string Format(Vector3 value)
        {
            return string.Join(",", Format(value.x), Format(value.y), Format(value.z));
        }

        public static string Format(Vector3Int value)
        {
            return string.Join(",", Format(value.x), Format(value.y), Format(value.z));
        }

        public static bool ParseBool(string value)
        {
            return bool.Parse(value);
        }

        private static byte ParseByte(string value)
        {
            return byte.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static Color ParseColor(string value)
        {
            var parts = ParseComponents(value, 4);
            return new(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3]));
        }

        public static Color32 ParseColor32(string value)
        {
            var parts = ParseComponents(value, 4);
            return new(ParseByte(parts[0]), ParseByte(parts[1]), ParseByte(parts[2]), ParseByte(parts[3]));
        }

        private static string[] ParseComponents(string value, int count)
        {
            var parts = value.Split(',');
            if (parts.Length != count)
                throw new FormatException($"Expected {count} components.");

            return parts;
        }

        public static DateTime ParseDateTime(string value)
        {
            return DateTime.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        public static double ParseDouble(string value)
        {
            return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static float ParseFloat(string value)
        {
            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static int ParseInt(string value)
        {
            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static long ParseLong(string value)
        {
            return long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static Quaternion ParseQuaternion(string value)
        {
            var parts = ParseComponents(value, 4);
            return new(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3]));
        }

        public static string ParseString(string value)
        {
            return value;
        }

        public static TimeSpan ParseTimeSpan(string value)
        {
            return TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture);
        }

        public static uint ParseUint(string value)
        {
            return uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static ulong ParseUlong(string value)
        {
            return ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static Vector2 ParseVector2(string value)
        {
            var parts = ParseComponents(value, 2);
            return new(ParseFloat(parts[0]), ParseFloat(parts[1]));
        }

        public static Vector2Int ParseVector2Int(string value)
        {
            var parts = ParseComponents(value, 2);
            return new(ParseInt(parts[0]), ParseInt(parts[1]));
        }

        public static Vector3 ParseVector3(string value)
        {
            var parts = ParseComponents(value, 3);
            return new(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[2]));
        }

        public static Vector3Int ParseVector3Int(string value)
        {
            var parts = ParseComponents(value, 3);
            return new(ParseInt(parts[0]), ParseInt(parts[1]), ParseInt(parts[2]));
        }
    }
}
