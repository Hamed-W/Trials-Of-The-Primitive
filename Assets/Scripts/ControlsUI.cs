using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlRebindUI : MonoBehaviour
{
    [SerializeField] private string actionName;
    [SerializeField] private int bindingIndex;

    [SerializeField] private TMP_Text bindingText;

    [SerializeField] private Button rebindButton;

    private InputAction action;

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    private void Start()
    {
        if (SettingsManager.Instance == null) return;

        action = SettingsManager.Instance.inputActions.FindAction(actionName);
        UpdateBindingText();
    }

    public void StartRebind()
    {
        if (action == null || action.bindings[bindingIndex].isComposite) return;

        action.Disable();
        bindingText.text = "Press a key...";
        rebindButton.interactable = false;

        rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(operation => FinishRebind())
            .OnComplete(operation => FinishRebind())
            .Start();
    }

    private void FinishRebind()
    {
        rebindOperation?.Dispose();
        rebindOperation = null;
        action.Enable();
        rebindButton.interactable = true;

        UpdateBindingText();
        SettingsManager.Instance.SaveBindings();
    }

    public void UpdateBindingText()
    {
        bindingText.text = action.GetBindingDisplayString(bindingIndex);
    }

    private void OnDestroy()
    {
        rebindOperation?.Dispose();
    }
}