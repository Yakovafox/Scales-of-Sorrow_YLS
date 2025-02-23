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
        Debug.Log("Player ID");
        playerInput.gameObject.GetComponent<PlayerController>().SetPlayerID(playerID);
        Debug.Log("Player Position");
        playerInput.gameObject.GetComponent<PlayerController>().SetPosition(new Vector3(8, 0.35f, 5));
    }

}
