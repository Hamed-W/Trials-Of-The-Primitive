using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EntityUIHandler : MonoBehaviour
{
    [SerializeField ] private TMP_Text levelText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Transform anchor;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (anchor == null || mainCamera == null)
            return;

        transform.position = anchor.position;

        Vector3 direction = mainCamera.transform.position - transform.position;
        direction.y = 0f;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void SetLevel(int level)
    {
        levelText.text = "Level " + level.ToString();
    }

    public void SetMaxHP(float maxHP)
    {
        hpSlider.maxValue = maxHP;
    }

    public void UpdateHP(float HP)
    {
        hpSlider.value = HP;
    }
}
