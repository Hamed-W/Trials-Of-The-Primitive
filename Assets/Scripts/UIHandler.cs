using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private CraftingManager craftingManager;
    [SerializeField] private Inventory inventory;

    [SerializeField] private bool inGame = false;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject titleScreenPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject inGameMenuPanel;

    [SerializeField] private List<GameObject> activePanels = new List<GameObject>();

    private bool paused = false;

    [SerializeField] private CinemachineInputProvider firstPersonInput;
    [SerializeField] private CinemachineInputProvider thirdPersonInput;



    // Start is called before the first frame update
    void Start()
    {
        if (inGame) 
        { 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (titleScreenPanel != null && titleScreenPanel.activeSelf)
            {
                activePanels.Add(titleScreenPanel);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnOpenInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryPanel.activeSelf;

        if (!inventoryPanel.activeSelf)
        {
            craftingManager.ClearCraftingItems();
        }
    }

    void OnSplitStack(InputValue value)
    {
        if (value.isPressed)
        {
            inventory.splitStack = true;
        }
        else
        {
            inventory.splitStack = false;
        }
    }
    public void ClearPanels()
    {
        foreach(GameObject panel in activePanels)
        {
            panel.SetActive(false);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }


    public void OnMenu()
    {
        if (!paused) OpenInGameMenuPanel();
        else CloseInGameMenuPanel();
    }

    public void OnOpenTitleScreenPanel()
    {
        ClearPanels();
        titleScreenPanel.SetActive(true);
        activePanels.Add(titleScreenPanel);
    }
    public void OpenSettingsPanel()
    {
        ClearPanels();
        settingsPanel.SetActive(true);
        activePanels.Add(settingsPanel);
    }

    public void OpenInGameMenuPanel()
    {
        firstPersonInput.enabled = false;
        thirdPersonInput.enabled = false;
        paused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ClearPanels();
        inGameMenuPanel.SetActive(true);
        activePanels.Add(inGameMenuPanel);
    }

    public void CloseInGameMenuPanel()
    {
        firstPersonInput.enabled = true;
        thirdPersonInput.enabled = true;
        paused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ClearPanels();
    }
}
