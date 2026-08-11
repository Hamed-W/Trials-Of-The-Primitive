using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text volumeAmount;
    [SerializeField] private TMP_Text sensitivityAmount;
    [SerializeField] private TMP_Text fovAmount;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider fovSlider;





    // Start is called before the first frame update
    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            volumeSlider.value = SettingsManager.Instance.volume;
            sensitivitySlider.value = SettingsManager.Instance.sensitivity;
            fovSlider.value = SettingsManager.Instance.fov;
        }

        UpdateText();

    }

    private void UpdateText()
    {
        OnVolumeUpdate(volumeSlider.value);
        OnSensitivityUpdate(sensitivitySlider.value);
        OnFOVUpdate(fovSlider.value);
    }



    public void OnVolumeUpdate(float volume)
    {
        volumeAmount.text = Mathf.RoundToInt(volume).ToString() + "%";
    }



    public void OnSensitivityUpdate(float sensitivity)
    {
        sensitivityAmount.text = (Mathf.Round(sensitivity * 100) / 100).ToString() + "x";
    }

    public void OnFOVUpdate(float fov)
    {
        fovAmount.text = Mathf.RoundToInt(fov).ToString();
    }


    public void OnSave()
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.SaveSettings(volumeSlider.value, sensitivitySlider.value, fovSlider.value);
    }

    public void OnResetControls()
    {
        if (SettingsManager.Instance == null)
            return;

        SettingsManager.Instance.ResetBindings();

        ControlRebindUI[] controls = FindObjectsByType<ControlRebindUI>(FindObjectsSortMode.None);

        foreach (ControlRebindUI control in controls)
        {
            control.UpdateBindingText();
        }
    }


}
