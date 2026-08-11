using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Saved Settings")]
    [Range(0f, 100f)]
    public float volume = 100f;
    public float sensitivity = 1f;
    public float fov = 75f;

    [Header("Input")]
    public InputActionAsset inputActions;

    private string bindingChanges;

    public event Action SettingsChanged;
    public event Action BindingsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyVolume();
    }

    public void SaveSettings(float newVolume, float newSensitivity, float newFOV)
    {
        volume = newVolume;
        sensitivity = newSensitivity;
        fov = newFOV;

        ApplyVolume();
        SettingsChanged?.Invoke();
    }

    public void ApplyVolume()
    {
        AudioListener.volume = volume / 100f;
    }

    public void SaveBindings()
    {
        bindingChanges = inputActions.SaveBindingOverridesAsJson();

        BindingsChanged?.Invoke();
    }

    public void ApplySavedBindings(InputActionAsset targetActions)
    {
        targetActions.RemoveAllBindingOverrides();
        if (string.IsNullOrEmpty(bindingChanges)) return;
        targetActions.LoadBindingOverridesFromJson(bindingChanges);
    }

    public void ResetBindings()
    {
        inputActions.RemoveAllBindingOverrides();
        bindingChanges = "";

        BindingsChanged?.Invoke();
    }

}