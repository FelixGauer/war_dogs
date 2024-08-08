using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int currentPlayer;
    public int currentAlivePlayers;

    private void Awake()
    {
        currentAlivePlayers = currentPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentAlivePlayers <= 0)
        {
        }
    }
}
