using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    private static Func<Task<bool>> _confirmQuitAsync;
    private static bool _isQuitAllowed;
    private static Action<Exception> _onQuitFailed;
    private static Func<Task> _prepareQuitAsync;
    private static object _quitSession = new();

    public static bool IsExiting { get; private set; }
    public static bool IsQuitPending { get; private set; }

    public static event Action OnExitChanged;

    public static void ConfigureQuit(Func<Task<bool>> confirmQuitAsync, Func<Task> prepareQuitAsync, Action<Exception> onQuitFailed)
    {
        if (IsQuitPending)
            throw new InvalidOperationException("QUIT IS ALREADY PENDING.");
        if (confirmQuitAsync == null)
            throw new ArgumentNullException(nameof(confirmQuitAsync));
        if (prepareQuitAsync == null)
            throw new ArgumentNullException(nameof(prepareQuitAsync));

        _confirmQuitAsync = confirmQuitAsync;
        _prepareQuitAsync = prepareQuitAsync;
        _onQuitFailed = onQuitFailed;
        Application.wantsToQuit -= OnApplicationWantsToQuit;
        Application.wantsToQuit += OnApplicationWantsToQuit;
        Application.quitting -= OnApplicationQuitting;
        Application.quitting += OnApplicationQuitting;
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

    private static void NotifyExitChanged()
    {
        if (OnExitChanged == null)
            return;

        foreach (Action callback in OnExitChanged.GetInvocationList())
        {
            try
            {
                callback();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private static void OnApplicationQuitting()
    {
        _quitSession = new();
    }

    private static bool OnApplicationWantsToQuit()
    {
        if (_isQuitAllowed)
            return true;

        Quit();
        return false;
    }

    public static async void Quit()
    {
        if (IsQuitPending)
            return;
        if (_confirmQuitAsync == null)
        {
            QuitImmediately();
            return;
        }

        IsQuitPending = true;
        var quitSession = _quitSession;
        try
        {
            // wantsToQuit의 현재 요청이 반환된 뒤 확인과 실제 종료를 시작한다.
            await Task.Yield();
            if (quitSession != _quitSession)
                return;

            var isConfirmed = await _confirmQuitAsync();
            if (quitSession != _quitSession)
                return;
            if (isConfirmed == false)
            {
                IsQuitPending = false;
                return;
            }

            IsExiting = true;
            NotifyExitChanged();
            await _prepareQuitAsync();
            if (quitSession != _quitSession)
                return;

            _isQuitAllowed = true;
            QuitImmediately();
        }
        catch (Exception e)
        {
            if (quitSession != _quitSession)
                return;

            _isQuitAllowed = false;
            IsExiting = false;
            IsQuitPending = false;
            NotifyExitChanged();
            Debug.LogException(e);
            try
            {
                _onQuitFailed?.Invoke(e);
            }
            catch (Exception callbackException)
            {
                Debug.LogException(callbackException);
            }
        }
    }

    private static void QuitImmediately()
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetQuit()
    {
        Application.wantsToQuit -= OnApplicationWantsToQuit;
        Application.quitting -= OnApplicationQuitting;
        _confirmQuitAsync = null;
        _isQuitAllowed = false;
        _onQuitFailed = null;
        _prepareQuitAsync = null;
        _quitSession = new();
        IsExiting = false;
        IsQuitPending = false;
        OnExitChanged = null;
    }
}
