using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private List<GameObject> children = new List<GameObject>();
    [SerializeField] private bool playersExist;


    public delegate void delegate_playerDefeated();
    public static event delegate_playerDefeated OnPlayerDefeated;


    public void AddChild(GameObject child)
    {
        children.Add(child);
        playersExist = true;
    }
    public void RemoveChild(GameObject child)
    {
        children.Remove(child);
    }

    private void Update()
    {
        if (children.Count == 0 & playersExist)
        {
            OnPlayerDefeated();
            playersExist = false;
        }
    }
}
