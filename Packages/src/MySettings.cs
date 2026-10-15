using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static partial class MySettings
{
    private static string _accountPath;
    private static Store _cloudStore;
    private static Store _localStore;

    private static Store CloudStore => GetStore(ref _cloudStore, "cloud.settings");
    private static Store LocalStore => GetStore(ref _localStore, "local.settings");

    private static Store GetStore(ref Store store, string fileName)
    {
        if (_accountPath == null)
            throw new InvalidOperationException("Call MySettings.Initialize before accessing settings.");

        store ??= new(Path.Combine(_accountPath, fileName));
        SaverMonoBehaviour.EnsureCreated();
        return store;
    }

    public static void Initialize(string accountId)
    {
        if (_accountPath != null)
            return;

        if (string.IsNullOrWhiteSpace(accountId) || (accountId == ".") || (accountId == "..") || (accountId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) || accountId.Contains('/') || accountId.Contains('\\') || accountId.EndsWith(".", StringComparison.Ordinal) || accountId.EndsWith(" ", StringComparison.Ordinal))
            throw new ArgumentException("Account ID must be a non-empty directory name.", nameof(accountId));

        _accountPath = Path.Combine(Application.persistentDataPath, "Accounts", accountId);
    }

    private static void SaveAll()
    {
        foreach (var store in new[] { _localStore, _cloudStore })
        {
            if (store == null)
                continue;

            try
            {
                store.Save();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private static Task SaveAsync()
    {
        return Task.WhenAll(_localStore?.SaveAsync() ?? Task.CompletedTask, _cloudStore?.SaveAsync() ?? Task.CompletedTask);
    }
}
