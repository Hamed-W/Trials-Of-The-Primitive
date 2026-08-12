using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip nightMusic;
    [SerializeField] private AudioClip[] dayMusic;
    [SerializeField] private AudioClip deathMusic;
    [SerializeField] private AudioClip victoryMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip grassFootstep;
    [SerializeField] private AudioClip sandFootstep;

    [SerializeField] private AudioClip swordSwing;
    [SerializeField] private AudioClip swordImpact;
    [SerializeField] private AudioClip healUse;



    private void Start()
    {
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        if (SettingsManager.Instance == null)
            return;

        musicSource.volume = SettingsManager.Instance.musicVolume / 100f;
        sfxSource.volume = SettingsManager.Instance.sfxVolume / 100f;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySwordSwing()
    {
        PlaySFX(swordSwing);
    }

    public void PlaySwordImpact()
    {
        PlaySFX(swordImpact);
    }

    public void PlayHeal()
    {
        PlaySFX(healUse);
    }

    public void PlayFootstep(Biome biome)
    {
        if (biome == Biome.Grass) PlaySFX(grassFootstep);
        else if (biome == Biome.Sand) PlaySFX(sandFootstep);
    }


    //Gets a random index to play one of the daytime music at random.
    public void PlayDayMusic()
    {
        if (dayMusic == null || dayMusic.Length == 0) return;

        AudioClip clip = dayMusic[Random.Range(0, dayMusic.Length)];
        PlayMusic(clip, false);
    }

    public void PlayNightMusic()
    {
        PlayMusic(nightMusic, true);
    }

    public void PlayDeathMusic()
    {
        PlayMusic(deathMusic, false);
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic, false);
    }

    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (clip == null) return;

        musicSource.Stop();

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
}