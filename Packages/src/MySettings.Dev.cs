using System;
using UnityEngine;

public static partial class MySettings
{
    public static class Dev
    {
        public static T GetEnum<T>(string name) where T : Enum
        {
            return GetEnum<T>(name, default);
        }

        public static T GetEnum<T>(string name, T defValue) where T : Enum
        {
            return MyEnv.IsDevelopmentBuild() && Enum.TryParse(typeof(T), PlayerPrefs.GetString("d." + name, ""), out var value) ? (T)value : defValue;
        }

        public static bool GetFlag(string name)
        {
            return GetFlag(name, false);
        }

        public static bool GetFlag(string name, bool defValue)
        {
            return MyEnv.IsDevelopmentBuild() && ((PlayerPrefs.GetInt("d." + name, defValue ? 1 : 0) != 0) || System.IO.File.Exists("$" + name));
        }

        public static void SetEnum<T>(string name, T value) where T : Enum
        {
            PlayerPrefs.SetString("d." + name, value.ToString("G"));
        }

        public static void SetFlag(string name, bool value)
        {
            PlayerPrefs.SetInt("d." + name, value ? 1 : 0);
        }
    }
}
