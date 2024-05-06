using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guns", menuName = "Gun", order = 1)]
public class GunsScriptableObjects : ScriptableObject
{
    public int damage;
    public float fireRate;
    public float spread;
    public float range;
    public float reloadTime;
    public float timeBetweenShots;
    public int magazineSize; 
    public int bulletsPerTap;
    public bool allowButtonHold;
    public GameObject gunPrefab;
    public bool bulletObject;
    public float recoil;
    
    //Trail Renderer
    public Material material;
    public AnimationCurve animationCurve;
    public float duration;
    
    // public Gradient minColor;
    // public Gradient maxColor;
    public Color emissionColor;
}
