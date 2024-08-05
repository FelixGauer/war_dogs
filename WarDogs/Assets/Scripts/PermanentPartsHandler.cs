using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PermanentPartsHandler : MonoBehaviour
{
    
    public float health;
    public bool isDestroyed = false;
    
    [Header("Repairing")]
    public float repairAmount;
    public bool isRepairing;
    public PlayerInput playerInput;
    
    void Update()
    {
        if(health <= 0)
        {
            isDestroyed = true;
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Colliding");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is here");
            playerInput = other.GetComponentInParent<PlayerInput>();

            if (playerInput != null)
            {
                if (playerInput.actions["Repair"].IsPressed())
                {
                    Debug.Log("Repairing");
                    isRepairing = true;
                    health += repairAmount * Time.deltaTime; 
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInput = null;
            isRepairing = false;
        }
    }
}
