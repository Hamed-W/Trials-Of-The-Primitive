using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public InputActionReference lookAction;

    public float sensitivity = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        ApplySensitivity();
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void ApplySensitivity()
    {
        lookAction.action.ApplyBindingOverride(new InputBinding{overrideProcessors =$"scaleVector2(x={sensitivity},y={sensitivity})"});
    }
}
