using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoining : MonoBehaviour
{
    private PlayerInputManager inputManager;

    private void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerID = playerInput.playerIndex;
        playerInput.gameObject.GetComponent<PlayerController>().SetPlayerID(playerID);
    }

}
