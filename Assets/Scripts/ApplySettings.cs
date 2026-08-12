using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ApplySettings : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInput cameraPlayerInput;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private AudioManager audioManager;

    private void Start()
    {
        if (SettingsManager.Instance == null)
            return;

        ApplyBindings();
        ApplySoundSettings();
        ApplyGameSettings();

        SettingsManager.Instance.BindingsChanged += ApplyBindings;
        SettingsManager.Instance.SettingsChanged += ApplySoundSettings;
        SettingsManager.Instance.SettingsChanged += ApplyGameSettings;
    }

    //For transitioning between game scene and menu scene.
    private void OnDestroy()
    {
        if (SettingsManager.Instance == null)
            return;

        SettingsManager.Instance.BindingsChanged -= ApplyBindings;
        SettingsManager.Instance.SettingsChanged -= ApplySoundSettings;
        SettingsManager.Instance.SettingsChanged -= ApplyGameSettings;
    }

    private void ApplyBindings()
    {
        SettingsManager.Instance.ApplySavedBindings(playerInput.actions);
        SettingsManager.Instance.ApplySavedBindings(cameraPlayerInput.actions);
    }

    private void ApplySoundSettings()
    {
        SettingsManager.Instance.ApplyVolume();
        audioManager.ApplyVolumes();
    }

    private void ApplyGameSettings()
    {
        cameraManager.sensitivity = SettingsManager.Instance.sensitivity;
        cameraManager.ApplySensitivity();
        cameraManager.ApplyFOV(SettingsManager.Instance.fov);
    }
}