using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Guns : NetworkBehaviour
{
    [Header("Gun Stats scriptable object")]
    public GunsScriptableObjects gunStats;
    
    //Gun stats
    [Header("Guns General Stats")]
    private int bulletsLeft, bulletsShot;
    public Transform gunTransform;
    private Vector3 direction;
    private float x;
    private float y;
    public GameObject bulletTrailGo;
    public Slider gunReloadBar;
    public float colorIntensity;
    public float throwSpeed;
    public Rigidbody rb;
    
    [Header("Bools")]
    bool shooting, readyToShoot, reloading;

    public bool isLobbyCreated;
    
    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public LayerMask whatIsEnemy;

    [Header("Graphics")]
    public GameObject bulletHoleGraphic;
    
    private void Awake()
    {
        gunReloadBar = GameObject.Find("ReloadSlider").GetComponent<Slider>();
        bulletsLeft = gunStats.magazineSize;
        readyToShoot = true;
    }
    
    public bool IsLobbyCreated()
    {
        return NetworkManager.Singleton.IsServer;
    }
    
    private void Update()
    {
        isLobbyCreated = IsLobbyCreated();
        IsLobbyCreated();
        

        if (IsLobbyCreated())
        {
            MyInput();

            //change this to gun
            // direction = fpsCam.transform.forward + new Vector3(x, y, 0);
            direction = fpsCam.transform.forward + new Vector3(x, y, 0);

            Quaternion rotation = Quaternion.LookRotation(direction);
            gunTransform.rotation = rotation;

            gunReloadBar.maxValue = gunStats.magazineSize;
            gunReloadBar.value = bulletsLeft;
        }
    }
    private void MyInput()
    {
        if (gunStats.allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < gunStats.magazineSize && !reloading)
        {
            StartCoroutine(Reload());
        }

        
        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0){
            bulletsShot = gunStats.bulletsPerTap;
            Shoot();
        }
    }
    
    private void Shoot()
    {
        readyToShoot = false;
        RaycastHit rayHit;

        //Spread
        x = Random.Range(-gunStats.spread, gunStats.spread);
        y = Random.Range(gunStats.spread, gunStats.spread);

        //Calculate Direction with Spread
        direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        //RayCast
        if (Physics.Raycast(attackPoint.transform.position, direction, out rayHit, gunStats.range, whatIsEnemy))
        {    

            if (rayHit.collider.CompareTag("Enemy"))
            {
                rayHit.collider.GetComponent<EnemyAi>().TakeDamage(gunStats.damage);
            }
        }

        //TODO: ShakeCamera

        if (gunStats.bulletObject)
        {
            Debug.Log("11");
            bulletTrailGo = PoolManager.instance.GetPooledBulletObject();
        }
        else
        {
            bulletTrailGo = PoolManager.instance.GetPooledBulletsTrails();
        }
        
        // Using Gradiant
         // ParticleSystem.MainModule bulletTrailParticle = bulletTrailGo.GetComponent<ParticleSystem>().main;
         // bulletTrailParticle.startColor = new ParticleSystem.MinMaxGradient(gunStats.minColor, gunStats.maxColor);

         
         //TODO move this to when switching weapons
        Material material = gunStats.material;
        material.SetColor("_EmissionColor", gunStats.emissionColor * colorIntensity);
        
        
        if (bulletTrailGo != null && !gunStats.bulletObject)
        {
            bulletTrailGo.transform.position = attackPoint.position;
            bulletTrailGo.transform.rotation = attackPoint.rotation;
            bulletTrailGo.SetActive(true);
            
        }
        else
        {
            bulletTrailGo.transform.position = attackPoint.position;
            bulletTrailGo.transform.rotation = attackPoint.rotation;
            rb = bulletTrailGo.GetComponent<Rigidbody>();
            rb.velocity = attackPoint.forward * throwSpeed;
            bulletTrailGo.SetActive(true);
        }
        
        if (bulletTrailGo != null)
        {
            bulletTrailGo.transform.position = attackPoint.position;
            bulletTrailGo.transform.rotation = Quaternion.LookRotation(direction); // Set the rotation to match the direction
            bulletTrailGo.SetActive(true);
        }
        
        //Graphics
        Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0, 180, 0));
        // Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", gunStats.fireRate);
        
        if(bulletsShot > 0 && bulletsLeft > 0)
        Invoke("Shoot", gunStats.fireRate);
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }
    private IEnumerator Reload()
    {
        reloading = true;
        float elapsedTime = 0f;

        while (elapsedTime < gunStats.reloadTime)
        {
            gunReloadBar.value = Mathf.Lerp(0, gunStats.magazineSize, elapsedTime / gunStats.reloadTime);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        ReloadFinished();
    }
    private void ReloadFinished()
    {
        bulletsLeft = gunStats.magazineSize;
        reloading = false;
    }
    
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        // Draw the raycast gizmo
        Gizmos.color = Color.red; // Set color for gizmo
        Vector3 startPos = fpsCam.transform.position; // Start position of the raycast
        Vector3 direction = fpsCam.transform.forward; // Direction of the raycast
        Gizmos.DrawLine(startPos, startPos + direction * gunStats.range); // Draw the raycast as a line
    }
}
