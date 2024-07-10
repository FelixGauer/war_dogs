using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateBreaches : MonoBehaviour
{
    private void OnEnable()
    {
        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.activeSpawnPoints.Add(this.gameObject);
        }
    }

    private void OnDisable()
    {
        WaveSpawner.instance.activeSpawnPoints.Remove(this.gameObject);
    }
}
