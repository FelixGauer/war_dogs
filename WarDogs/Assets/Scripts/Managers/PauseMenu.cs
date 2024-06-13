using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public WaveSpawner waveSpawner;

    public void StartWaves()
    {
        waveSpawner.enabled = true; //Add Breakable wall spawns as well, stop them from starting 
    }
}
