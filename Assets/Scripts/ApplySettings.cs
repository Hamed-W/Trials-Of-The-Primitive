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

        // Subscribes the local application of the settings to SettingsManager so that when the user makes a change on the settings panel it will automatically update the appropriate fields through here.
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

    //Applies the control binding changes to the playerInput references.
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