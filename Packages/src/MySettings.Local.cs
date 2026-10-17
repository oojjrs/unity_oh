using System;
using UnityEngine;

public static partial class MySettings
{
    public static class Local
    {
        public static void DeleteAll()
        {
            LocalStore.DeleteAll();
        }

        public static void DeleteKey(string key)
        {
            LocalStore.DeleteKey(key);
        }

        public static bool GetBool(string key, bool defValue = false)
        {
            return LocalStore.Get(key, defValue, Values.ParseBool);
        }

        public static Color GetColor(string key, Color defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseColor);
        }

        public static Color32 GetColor32(string key, Color32 defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseColor32);
        }

        public static DateTime GetDateTime(string key, DateTime defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseDateTime);
        }

        public static double GetDouble(string key, double defValue = 0)
        {
            return LocalStore.Get(key, defValue, Values.ParseDouble);
        }

        public static T GetEnum<T>(string key, T defValue = default) where T : struct, Enum
        {
            return LocalStore.Get(key, defValue, Enum.Parse<T>);
        }

        public static float GetFloat(string key, float defValue = 0)
        {
            return LocalStore.Get(key, defValue, Values.ParseFloat);
        }

        public static int GetInt(string key, int defValue = 0)
        {
            return LocalStore.Get(key, defValue, Values.ParseInt);
        }

        public static long GetLong(string key, long defValue = 0)
        {
            return LocalStore.Get(key, defValue, Values.ParseLong);
        }

        public static Quaternion GetQuaternion(string key, Quaternion? defValue = null)
        {
            return LocalStore.Get(key, defValue ?? Quaternion.identity, Values.ParseQuaternion);
        }

        public static string GetString(string key, string defValue = "")
        {
            return LocalStore.Get(key, defValue, Values.ParseString);
        }

        public static TimeSpan GetTimeSpan(string key, TimeSpan defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseTimeSpan);
        }

        public static uint GetUint(string key, uint defValue = 0)
        {
            return LocalStore.Get(key, defValue, Values.ParseUint);
        }

        public static ulong GetUlong(string key, ulong defValue = 0)
        {
            return LocalStore.Get(key, defValue, Values.ParseUlong);
        }

        public static Vector2 GetVector2(string key, Vector2 defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseVector2);
        }

        public static Vector2Int GetVector2Int(string key, Vector2Int defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseVector2Int);
        }

        public static Vector3 GetVector3(string key, Vector3 defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseVector3);
        }

        public static Vector3Int GetVector3Int(string key, Vector3Int defValue = default)
        {
            return LocalStore.Get(key, defValue, Values.ParseVector3Int);
        }

        public static bool HasKey(string key)
        {
            return LocalStore.HasKey(key);
        }

        public static void Save()
        {
            LocalStore.Save();
        }

        public static void SetBool(string key, bool value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetColor(string key, Color value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetColor32(string key, Color32 value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetDateTime(string key, DateTime value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetDouble(string key, double value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetEnum<T>(string key, T value) where T : struct, Enum
        {
            LocalStore.Set(key, value.ToString("G"));
        }

        public static void SetFloat(string key, float value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetInt(string key, int value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetLong(string key, long value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetQuaternion(string key, Quaternion value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetString(string key, string value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetTimeSpan(string key, TimeSpan value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetUint(string key, uint value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetUlong(string key, ulong value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetVector2(string key, Vector2 value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetVector2Int(string key, Vector2Int value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetVector3(string key, Vector3 value)
        {
            LocalStore.Set(key, Values.Format(value));
        }

        public static void SetVector3Int(string key, Vector3Int value)
        {
            LocalStore.Set(key, Values.Format(value));
        }
    }
}
