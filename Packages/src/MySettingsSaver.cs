using System;
using System.Threading.Tasks;
using UnityEngine;

public static partial class MySettings
{
    private sealed class SaverMonoBehaviour : MonoBehaviour
    {
        private const double SaveIntervalSeconds = 0.3;

        private static SaverMonoBehaviour __instance;

        private bool _isQuitting;
        private double _nextSaveTime;
        private bool _saveFailed;
        private Task _saveTask = Task.CompletedTask;

        public static void EnsureCreated()
        {
            if (__instance != null)
                return;

            foreach (var saver in Resources.FindObjectsOfTypeAll<SaverMonoBehaviour>())
            {
                if (saver.gameObject.scene.IsValid())
                {
                    __instance = saver;
                    return;
                }
            }

            var gameObject = new GameObject(nameof(MySettings)) { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(gameObject);
            __instance = gameObject.AddComponent<SaverMonoBehaviour>();
        }

        private void Awake()
        {
            _nextSaveTime = Time.realtimeSinceStartupAsDouble + SaveIntervalSeconds;
            Application.quitting += OnQuitting;
        }

        private void OnDestroy()
        {
            Application.quitting -= OnQuitting;
            if (__instance == this)
            {
                CompleteSave();
                SaveAll();
                __instance = null;
            }
        }

        private void Update()
        {
            if (_isQuitting || (Time.realtimeSinceStartupAsDouble < _nextSaveTime))
                return;

            _nextSaveTime = Time.realtimeSinceStartupAsDouble + SaveIntervalSeconds;
            if (_saveTask.IsCompleted == false)
                return;

            CompleteSave();
            _saveTask = SaveAsync();
        }

        private void CompleteSave()
        {
            try
            {
                _saveTask.GetAwaiter().GetResult();
                _saveFailed = false;
            }
            catch (Exception e)
            {
                if (_saveFailed == false)
                    Debug.LogException(e);

                _saveFailed = true;
            }
            finally
            {
                _saveTask = Task.CompletedTask;
            }
        }

        private void OnQuitting()
        {
            _isQuitting = true;
            CompleteSave();
            SaveAll();
        }
    }
}
