using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;

    private void Start()
    {
        healthSlider.maxValue = playerStats.maxHealth;

        hungerSlider.maxValue = playerStats.maxHunger;

        Refresh();
    }

    private void OnEnable()
    {
        playerStats.StatsChanged += Refresh;
    }

    private void OnDisable()
    {
        playerStats.StatsChanged -= Refresh;
    }

    private void Refresh()
    {
        healthSlider.maxValue = playerStats.maxHealth;

        healthSlider.value = playerStats.currentHealth;

        hungerSlider.value = playerStats.currentHunger;
    }
}