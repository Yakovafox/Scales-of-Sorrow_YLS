using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInputs : MonoBehaviour
{
    public static MenuInputs instance;

    public bool menuOpenCloseInput { get; private set; }

    private PlayerInput playerInput;
    private InputAction menuOpenClose;
    

    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }

        playerInput = GetComponent<PlayerInput>();
        menuOpenClose = playerInput.actions["MenuOpenClose"];


    }

    private void Update()
    {
        menuOpenCloseInput = menuOpenClose.WasPressedThisFrame();

    }
}
