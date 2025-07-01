using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;


public class VibrationService
{
    public bool isVibrationEnable = true;

    public void VibratePhone(HapticPatterns.PresetType vibrationType)
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        if (isVibrationEnable)
            HapticPatterns.PlayPreset(vibrationType);
#endif
    }

    public void VibratePhoneConstantly()
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        if (isVibrationEnable)
            HapticPatterns.PlayConstant(0.1f, 0.1f, 15f);
#endif
    }

    public void StopVibratoin()
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        if (isVibrationEnable)
            HapticPatterns.PlayConstant(0f, 0f, 0f);
#endif
    }
}