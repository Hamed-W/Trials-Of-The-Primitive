using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

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
    [SerializeField] private GameObject recipePanel;
    [SerializeField] private GameObject endGamePanel;

    [SerializeField] private List<GameObject> activePanels = new List<GameObject>();

    private bool paused = false;

    [SerializeField] private CinemachineInputProvider firstPersonInput;
    [SerializeField] private CinemachineInputProvider thirdPersonInput;

    [SerializeField] private TMP_Text endGameText;



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
        if (inventoryPanel == null) return;
        if (inGameMenuPanel.activeSelf) return;
        if (!inventoryPanel.activeSelf)
        {
            activePanels.Add(inventoryPanel);
            firstPersonInput.enabled = false;
            thirdPersonInput.enabled = false;
            inventory.inputEnabled = false;
        }
        else
        {
            firstPersonInput.enabled = true;
            thirdPersonInput.enabled = true;
            inventory.inputEnabled = true;
            craftingManager.ClearCraftingItems();
        }
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryPanel.activeSelf;
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
        activePanels.Clear();
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

    public void OpenRecipePanel()
    {
        ClearPanels();

        recipePanel.SetActive(true);
        activePanels.Add(recipePanel);
    }

    public void OpenInGameMenuPanel()
    {
        if (activePanels.Contains(inventoryPanel)) craftingManager.ClearCraftingItems();
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

    public void OpenEndGamePanel(bool won)
    {
        ClearPanels();
        endGamePanel.SetActive(true);
        endGameText.text = won ? "You Win" : "You Died";
        activePanels.Add(endGamePanel);

        Time.timeScale = 0f;
        firstPersonInput.enabled = false;
        thirdPersonInput.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
