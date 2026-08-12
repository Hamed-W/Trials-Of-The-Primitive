using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Material skyboxMaterial;

    [Range(0f, 1f)]
    [SerializeField] private float timeOfDay;

    [Header("Sunlight")]
    [SerializeField] private Light sunlight;
    [SerializeField] private AnimationCurve sunIntensity;
    [SerializeField] private Gradient sunColor;

    [Header("Moonlight")]
    [SerializeField] private Light moonlight;
    [SerializeField] private AnimationCurve moonIntensity;
    [SerializeField] private Gradient moonColor;



    [SerializeField] private Gradient skyColor;
    [SerializeField] private AnimationCurve skyExposure;

    [SerializeField] private Gradient ambientColor;
    [SerializeField] private AnimationCurve ambientIntensity;

    [SerializeField] private float dayLength = 120f;
    public float nightDuration;

    [SerializeField] private TMP_Text timeText;

    private bool isDay = false;
    private bool isNight = false;

    public int dayCount = 0;

    public event Action OnNightStarted;
    public event Action OnDayStarted;

    [SerializeField] private int daysToSurvive = 10;
    [SerializeField] private UIHandler uiHandler;
    [SerializeField] private AudioManager audioManager;

    void Awake()
    {
        nightDuration = dayLength * (1f - 0.9f + 0.25f);
    }

    private void Start()
    {

        timeOfDay = 0.25f;
        StartDay();
    }

    private void Update()
    {

        // Translates into in game time.

        timeOfDay += Time.deltaTime / dayLength;

        if (timeOfDay >= 1f) timeOfDay = 0f;

        // Updates the skybox based on the new time.
        UpdateSkybox();
        UpdateSunAndMoon();
        UpdateAmbientLight();


        // Checks for new day/night start.
        if (!isDay && (timeOfDay < 0.9f && timeOfDay >= 0.25f))
        {
            StartDay();
        }

        if (!isNight && (timeOfDay >= 0.9f || timeOfDay < 0.25f))
        {
            StartNight();
        }
        string dayOrNight = isDay ? "Day " : "Night ";

        timeText.text = dayOrNight + dayCount.ToString() + " - " + GetTimeString();
    }

    private void UpdateSkybox ()
    {
        skyboxMaterial.SetColor("_SkyTint", skyColor.Evaluate(timeOfDay));
        skyboxMaterial.SetFloat("_Exposure", skyExposure.Evaluate(timeOfDay));
    }

    private void UpdateSunAndMoon()
    {
        float sunAngle = timeOfDay * 360f - 90f;

        sunlight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        moonlight.transform.rotation = Quaternion.Euler(sunAngle + 180f, 170f, 0f);

        sunlight.intensity = sunIntensity.Evaluate(timeOfDay);
        moonlight.intensity = moonIntensity.Evaluate(timeOfDay);

        sunlight.color = sunColor.Evaluate(timeOfDay);
        moonlight.color = moonColor.Evaluate(timeOfDay);
    }

    private void UpdateAmbientLight()
    {
        RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDay);
        RenderSettings.ambientIntensity = ambientIntensity.Evaluate(timeOfDay);
    }

    public string GetTimeString()
    {
        float totalHours = timeOfDay * 24f;

        int hours = Mathf.FloorToInt(totalHours);
        int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);

        return $"{hours:00}:{minutes:00}";
    }


    private void StartNight()
    {
        isNight = true;
        isDay = false;

        Debug.Log("Night started");

        audioManager.PlayNightMusic();
        OnNightStarted?.Invoke();
    }

    private void StartDay()
    {
        isNight = false;
        isDay = true;

        dayCount++;

        Debug.Log($"Day {dayCount} started");

        // Checks if the player has won.
        if (dayCount > daysToSurvive)
        {
            uiHandler.OpenEndGamePanel(true);
            return;
        }

        audioManager.PlayDayMusic();
        OnDayStarted?.Invoke();
    }
}