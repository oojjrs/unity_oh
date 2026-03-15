using System;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioMixerExtensions
{
    public static void SetAmbienceVolumeSafety(this AudioMixer audioMixer, float value)
    {
        audioMixer.SetFloatSafety("AmbienceVolume", value);
    }

    [Obsolete("Use SetMusicVolumeSafety() instead.")]
    public static void SetBgmVolumeSafety(this AudioMixer audioMixer, float value)
    {
        audioMixer.SetFloatSafety("BgmVolume", value);
    }

    public static void SetFloatSafety(this AudioMixer audioMixer, string name, float value)
    {
        if (audioMixer != default)
        {
            if (value > 0)
                audioMixer.SetFloat(name, Mathf.Clamp01(value) * 40 - 40);
            else
                audioMixer.SetFloat(name, -80);
        }
        else
        {
            Debug.LogWarning($"SetFloatSafety IS FAILED: THE AUDIOMIXER IS NULL");
        }
    }

    public static void SetMasterVolumeSafety(this AudioMixer audioMixer, float value)
    {
        audioMixer.SetFloatSafety("MasterVolume", value);
    }

    public static void SetMusicVolumeSafety(this AudioMixer audioMixer, float value)
    {
        audioMixer.SetFloatSafety("MusicVolume", value);
    }

    [Obsolete("Use SetSoundVolumeSafety() instead.")]
    public static void SetSfxVolumeSafety(this AudioMixer audioMixer, float value)
    {
        audioMixer.SetFloatSafety("SfxVolume", value);
    }

    public static void SetSoundVolumeSafety(this AudioMixer audioMixer, float value)
    {
        audioMixer.SetFloatSafety("SoundVolume", value);
    }
}
