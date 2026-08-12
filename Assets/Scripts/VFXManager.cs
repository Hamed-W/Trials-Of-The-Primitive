using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VFXManager : MonoBehaviour
{
    public Volume sandVolume;
    public Volume grassVolume;

    public ParticleSystem sandFX;
    public ParticleSystem grassFX;

    public float transitionSpeed = 3f;
    public float targetFXRate = 300f;

    [SerializeField] private float biomeRefreshRate;
    private float biomeRefreshTimer;

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private Transform player;

    [SerializeField] private Biome biome = Biome.None;

    // Update is called once per frame
    void Update()
    {
        biomeRefreshTimer -= Time.deltaTime;
        if (biomeRefreshTimer <= 0f)
        {
            biome = mapGenerator.GetBiomeFromCoord(player.position);
            biomeRefreshTimer = biomeRefreshRate;
        }
        TransitionBiome(biome);
    }

    void TransitionBiome(Biome newBiome)
    {
        var sandEmission = sandFX.emission;
        var grassEmission = grassFX.emission;

        if (newBiome == Biome.Sand)
        {
            sandVolume.weight = Mathf.Lerp(sandVolume.weight, 1, transitionSpeed * Time.deltaTime);
            grassVolume.weight = Mathf.Lerp(grassVolume.weight, 0, transitionSpeed * Time.deltaTime);

            sandEmission.rateOverTime = Mathf.Lerp(sandFX.emission.rateOverTime.constant, (targetFXRate / 20), transitionSpeed * Time.deltaTime);
            grassEmission.rateOverTime = Mathf.Lerp(grassFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
        }
        else
        {
            sandVolume.weight = Mathf.Lerp(sandVolume.weight, 0, transitionSpeed * Time.deltaTime);
            grassVolume.weight = Mathf.Lerp(grassVolume.weight, 1, transitionSpeed * Time.deltaTime);

            sandEmission.rateOverTime = Mathf.Lerp(sandFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
            grassEmission.rateOverTime = Mathf.Lerp(grassFX.emission.rateOverTime.constant, (targetFXRate / 20), transitionSpeed * Time.deltaTime);
        }
    }


}
