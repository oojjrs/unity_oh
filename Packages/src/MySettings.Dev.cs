using System;
using UnityEngine;

public static partial class MySettings
{
    public static class Dev
    {
        public static T GetEnum<T>(string name) where T : struct, Enum
        {
            return GetEnum<T>(name, default);
        }

        public static T GetEnum<T>(string name, T defValue) where T : struct, Enum
        {
            return MyEnv.IsDevelopmentBuild() && Enum.TryParse<T>(PlayerPrefs.GetString("d." + name, ""), out var value) ? value : defValue;
        }

        public static bool GetFlag(string name)
        {
            return GetFlag(name, false);
        }

        public static bool GetFlag(string name, bool defValue)
        {
            return MyEnv.IsDevelopmentBuild() && ((PlayerPrefs.GetInt("d." + name, defValue ? 1 : 0) != 0) || System.IO.File.Exists("$" + name));
        }

        public static void SetEnum<T>(string name, T value) where T : struct, Enum
        {
            PlayerPrefs.SetString("d." + name, value.ToString("G"));
        }

        public static void SetFlag(string name, bool value)
        {
            PlayerPrefs.SetInt("d." + name, value ? 1 : 0);
        }
    }
}
