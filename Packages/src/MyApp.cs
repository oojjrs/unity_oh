using System;
using System.Collections.Generic;
using UnityEngine;

public static class MyApp
{
    [Flags]
    public enum VersionDisplayEnum
    {
        None = 0,
        Company = 1 << 0,
        Product = 1 << 1,
        Version = 1 << 2,
        Build = 1 << 3,
        All = Company | Product | Version | Build,
    }

    public static string GetVersionString(VersionDisplayEnum display = VersionDisplayEnum.All, string buildNumber = null)
    {
        var parts = new List<string>(4);

        if ((display & VersionDisplayEnum.Company) != 0)
            AddPart(Application.companyName, string.Empty);
        if ((display & VersionDisplayEnum.Product) != 0)
            AddPart(Application.productName, string.Empty);
        if ((display & VersionDisplayEnum.Version) != 0)
            AddPart(Application.version, "v");
        if ((display & VersionDisplayEnum.Build) != 0)
            AddPart(buildNumber, "Build ");

        return string.Join(" · ", parts);

        void AddPart(string value, string prefix)
        {
            if (string.IsNullOrWhiteSpace(value) == false)
                parts.Add(prefix + value.Trim());
        }
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
#endif
        {
            Application.Quit();
        }
    }
}
