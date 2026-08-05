using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnOpenInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
}
